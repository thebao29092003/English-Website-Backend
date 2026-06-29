using English.Website.Api.Dtos.BackendPythonDtos;

namespace English.Website.Application.Services.IServices
{
    public interface IBackendPythonService
    {
        Task ConvertAudioToPhonetic(RequestConvertAudioPhoneticDto requestConvert);
        Task<PhoneticCompareResponseDto?> ComparePhonetic(PhoneticCompareRequestDto requestCompare);
    }
}
