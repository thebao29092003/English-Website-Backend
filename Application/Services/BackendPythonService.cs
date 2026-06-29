using English.Website.Api.Dtos.BackendPythonDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;

namespace English.Website.Application.Services
{
    public class BackendPythonService : IBackendPythonService
    {
        private readonly HttpClient _httpClient;

        public BackendPythonService(
            HttpClient httpClient
        )
        {
            _httpClient = httpClient;
        }

        public async Task<PhoneticCompareResponseDto?> ComparePhonetic(PhoneticCompareRequestDto requestCompare)
        {
            var requestUrl = "http://localhost:8000/api/v1/phonetic-matching/compare";

            return await HttpHelper.SendPostJsonAsync<PhoneticCompareRequestDto, PhoneticCompareResponseDto>(
                _httpClient,
                requestUrl,
                requestCompare
            );

        }

        // này chỉ trả về statusCode và messgase là processing đi
        public async Task ConvertAudioToPhonetic(RequestConvertAudioPhoneticDto requestConvert)
        {
            var requestUrl = "http://localhost:8000/api/v1/convert-audio-phonetic/wav2vec2";

            var submitResult = await HttpHelper.SendPostJsonAsync<RequestConvertAudioPhoneticDto, ResponseConvertAudioPhoneticDto>(
               _httpClient,
               requestUrl,
               requestConvert,
               null
            );

            if (submitResult.statusCode != 202)
            {
                throw new BadRequestException($"convert text to phonetic faild");
            }
        }
    }
}
