using English.Website.Api.Dtos.AIDtos.AssemblyAIDto;
using English.Website.Api.Dtos.AIDtos.AzureSpeechDto;
using English.Website.Api.Dtos.AIDtos.DeepSeekDto;

namespace English.Website.Application.Services.IServices
{

    public interface IAssemblyAIService
    {

        Task<string> SubmitAudioAssemblyAI(AssemblyAIRequestDto requestDto);

        Task GetDataAssemblyAI(string transcriptId);

        Task<AssemblyAIResponseDto> CallAPIDeepSeek(string transcriptId);

        double CalculateFluencyScore(List<AssemblyAIWordDto> words, double? audioDuration);

    }
}
