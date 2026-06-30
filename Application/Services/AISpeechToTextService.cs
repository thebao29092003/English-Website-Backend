using Azure;
using English.Website.Api.Dtos.AIDtos.AssemblyAIDto;
using English.Website.Api.Dtos.BackendPythonDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace English.Website.Application.Services
{
    public class AISpeechToTextService
    {
        private readonly EnglishDBContext _dbContext;
        private readonly IBackendPythonService _backendPythonService;
        private readonly IAssemblyAIService _assemblyAIService;

        public AISpeechToTextService(
            EnglishDBContext dBContext,
            IBackendPythonService backendPythonService,
            IAssemblyAIService assemblyAIService
        )
        {
            _dbContext = dBContext;
            _backendPythonService = backendPythonService;
            _assemblyAIService = assemblyAIService;
        }

        public async Task Update(PythonPhonemeWebhookDto webhookData)
        {
            // cần xem xét viết hàm riêng bên asseblyAIService để call apiGetData để lấy ra wordsList chứ 
            // lấy lên từ database sẽ bị trường hợp bên webhook kia chưa kịp lưu vào database
            var assemblyAiResult = await _assemblyAIService.CallAPIGetDataAssemblyAI(webhookData.TranscriptId);

            var result =  _dbContext.AISpeechToText.FirstOrDefault(s => s.RecordingId == Guid.Parse(webhookData.RecordingId))
                ?? throw new BadRequestException("Not found AISpeechToText by RecordingId");

            var wordsList = (assemblyAiResult.Words) ?? throw new BadRequestException("Not found wordsList");

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
        }
    }
}
