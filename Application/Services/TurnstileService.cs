using English.Website.Api.Dtos.TurnstileDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;

namespace English.Website.Application.Services
{
    public class TurnstileService : ITurnstileService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;
        private readonly string _verifyUrl;

        public TurnstileService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _secretKey = configuration["Cloudfare:TurnStileKey"]
            _verifyUrl = configuration["Cloudfare:VerifyUrl"]
        }

        public async Task<bool> VerifyTokenAsync(string? token, string? remoteIp = null)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var requestBody = new TurnstileSiteVerifyRequest
            {
                Secret = _secretKey,
                Response = token,
                RemoteIp = remoteIp
            };

            var response = await HttpHelper.SendPostJsonAsync<TurnstileSiteVerifyRequest, TurnstileResponseDto>(
                _httpClient,
                _verifyUrl,
                requestBody
            );

            return response != null && response.Success;
        }
    }
}
