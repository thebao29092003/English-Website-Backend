using English.Website.Api.Dtos.AIDtos;

namespace English.Website.Application.Services.IServices
{
    public interface IDeepSeekService
    {
        Task<DeepSeekResponseDto?> AnalyzeSpeech(TranscriptRequestDto transcriptRequest);
    }
}
