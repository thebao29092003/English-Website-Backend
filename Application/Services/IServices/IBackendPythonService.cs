
using English.Website.Api.Dtos.AIDtos.BackendPythonDto;

namespace English.Website.Application.Services.IServices
{
    public interface IBackendPythonService
    {
        Task<ResponseConvertAudioPhoneticDto?> ConvertAudioToPhonetic(RequestConvertAudioPhoneticDto requestConvert);
    }
}
