using English.Website.Api.Dtos.BackendPythonDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using English.Website.Api.Hubs;
using System.Collections.Concurrent;

namespace English.Website.Application.Services
{
    public class AISpeechToTextService
    {
        private readonly EnglishDBContext _dbContext;
        private readonly IBackendPythonService _backendPythonService;
        private readonly IAssemblyAIService _assemblyAIService;
        private readonly IHubContext<AudioProcessingHub> _hubContext;

        // static giúp tất cả các instance của service trong cùng một tiến trình (process) dùng chung một dictionary.
        //  Tại sao phải dùng ConcurrentDictionary thay vì Dictionary thường
        // ConcurrentDictionary an toàn đa luồng (Thread-Safe): Lớp này được .NET tối ưu hóa để cho phép nhiều luồng
        // cùng lúc thêm, sửa, xóa dữ liệu một cách an toàn
        // điều quan trong ở đây là có thêm được key hay khong còn giá trị byte không quan trọng
        // sao cho nó nhỏ nhất để tốn ít data là được
        private static readonly ConcurrentDictionary<string, byte> _activeProcessing =
            new ConcurrentDictionary<string, byte>();

        public AISpeechToTextService(
            EnglishDBContext dBContext,
            IBackendPythonService backendPythonService,
            IAssemblyAIService assemblyAIService,
            IHubContext<AudioProcessingHub> hubContext
        )
        {
            _dbContext = dBContext;
            _backendPythonService = backendPythonService;
            _assemblyAIService = assemblyAIService;
            _hubContext = hubContext;
        }


        public async Task Update(PythonPhonemeWebhookDto webhookData)
        {
            var key = webhookData.RecordingId.ToLowerInvariant();
            if (!_activeProcessing.TryAdd(key, 0))
            {
                // Đang được xử lý song song bởi luồng khác, thoát để tránh trùng lặp
                return;
            }

            try
            {
                var recordingIdGuid = Guid.Parse(webhookData.RecordingId);
                var checkResult = _dbContext.AISpeechToText.FirstOrDefault(s => s.RecordingId == recordingIdGuid);
                if (checkResult != null && checkResult.PronunciationScore != null)
                {
                    // Đã được xử lý hoàn tất từ trước, thoát
                    return;
                }

                // cần xem xét viết hàm riêng bên asseblyAIService để call apiGetData để lấy ra wordsList chứ 
                // lấy lên từ database sẽ bị trường hợp bên webhook kia chưa kịp lưu vào database
                var assemblyAiResult = await _assemblyAIService.CallAPIGetDataAssemblyAI(webhookData.TranscriptId);

                // Kiểm tra xem kết quả lần 1 đã đạt yêu cầu chưa (ví dụ: null hoặc chưa xử lý xong)
                if (assemblyAiResult == null || assemblyAiResult.Words == null || !assemblyAiResult.Words.Any())
                {
                    // Nếu chưa có dữ liệu mong muốn, đợi 2 giây rồi thử lại lần cuối
                    await Task.Delay(2000);
                    assemblyAiResult = await _assemblyAIService.CallAPIGetDataAssemblyAI(webhookData.TranscriptId);
                }
                var wordsList = (assemblyAiResult.Words) ?? throw new BadRequestException("Not found wordsList");

                var result = _dbContext.AISpeechToText.FirstOrDefault(s => s.RecordingId == recordingIdGuid)
                    ?? throw new BadRequestException("Not found AISpeechToText by RecordingId");

                var wordListOnlyText = wordsList
                   .Select(w => w.Text)
                   .Where(text => !string.IsNullOrEmpty(text))
                   .ToList();

                var requestCompare = new PhoneticCompareRequestDto
                {
                    WordList = wordListOnlyText,
                    PhonemesList = webhookData.Phonemes
                };
                var compareResult = await _backendPythonService.ComparePhonetic(requestCompare);
                var wordScoresString = JsonSerializer.Serialize(compareResult?.WordScores);

                // Cập nhật chuỗi âm vị thu được từ audio bằng Wav2Vec2 vào DB
                result.PhoneticTranscript = webhookData.Phonemes;

                result.PronunciationScore = compareResult?.OverallAccuracy;
                result.WordsPronunciationScore = wordScoresString;
                await _dbContext.SaveChangesAsync();

                // Notify via SignalR to user group
                await _hubContext.Clients.Group(result.UserId.ToString().ToLowerInvariant()).SendAsync("ReceiveAudioStatus", new
                {
                    recordingId = result.RecordingId,
                    status = "Pronunciation_Analyzed",
                    data = new
                    {
                        pronunciationScore = compareResult?.OverallAccuracy
                    }
                });
            }
            finally
            {
                // Bắt buộc phải nằm trong finally: Để đảm bảo rằng dù đoạn code xử lý trong try có thành công hay xảy ra lỗi bất ngờ (Exception),
                // chiếc "khóa" của RecordingId đó chắc chắn sẽ được giải phóng khỏi Dictionary [
                _activeProcessing.TryRemove(key, out _);
            }
        }
    }
}
