using English.Website.Api.Dtos.AIDtos.AssemblyAIDto;
using English.Website.Api.Dtos.AIDtos.AzureSpeechDto;

namespace English.Website.Application.Services.IServices
{

    public interface IAssemblyAIService
    {

        Task<string> SubmitAudioAssemblyAI(AssemblyAIRequestDto requestDto);

        Task GetDataAssemblyAI(string transcriptId);

        Task<AssemblyAIResponseDto> CallAPIGetDataAssemblyAI(string transcriptId);

        FluencyAnalysisResult CalculateFluencyScore(List<AssemblyAIWordDto>? words, double? audioDuration);

    }
}
