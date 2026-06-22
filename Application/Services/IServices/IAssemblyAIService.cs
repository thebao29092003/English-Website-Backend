using English.Website.Api.Dtos.AIDtos.AssemblyAIDto;
using English.Website.Api.Dtos.AIDtos.AzureSpeechDto;
using English.Website.Api.Dtos.AIDtos.DeepSeekDto;

namespace English.Website.Application.Services.IServices
{

    public interface IAssemblyAIService
    {

        /// <summary>
        /// Gửi file âm thanh lên AssemblyAI để bắt đầu dịch bất đồng bộ.
        /// </summary>
        Task<string> SubmitAudio(AssemblyAIRequestDto requestDto);

        /// <summary>
        /// Lấy trạng thái dịch hiện tại của một bản ghi dựa trên ID.
        /// </summary>
        Task<AssemblyAIResponseDto> GetTranscriptStatusAsync(string transcriptId);

        /// <summary>
        /// Vòng lặp gọi kiểm tra trạng thái liên tục cho đến khi hoàn thành hoặc lỗi.
        /// </summary>
        Task<AssemblyAIResponseDto> PollUntilCompletedAsync(string transcriptId);

    }
}
