using AutoMapper;
using English.Website.Api.Dtos.AIDtos.AssemblyAIDto;
using English.Website.Api.Dtos.AIDtos.AzureSpeechDto;
using English.Website.Api.Dtos.AIDtos.BackendPythonDto;
using English.Website.Api.Dtos.AIDtos.DeepSeekDto;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Extend;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using English.Website.Domain.Entities.AI.AIModelText;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace English.Website.Application.Services
{
    public class BackendPythonService : IBackendPythonService
    {
        private readonly string _apiKey;
        private readonly string _webhookAuth;
        private readonly EnglishDBContext _englishDBContext;
        private readonly HttpClient _httpClient;
        private readonly IUserContextService _userContextService;

        public BackendPythonService(
            IConfiguration configuration,
            HttpClient httpClient,
            IUserContextService userContextService,
            EnglishDBContext englishDBContext
        )
        {
            _httpClient = httpClient;
            _apiKey = configuration["AI:AssemblyAIKey"]!;
            _webhookAuth = configuration["AI:AssemblyAIKey"]!;
            _userContextService = userContextService;
            _englishDBContext = englishDBContext;
        }

        // này chỉ trả về statusCode và messgase là processing đi
        public async Task<ResponseConvertAudioPhoneticDto?> ConvertAudioToPhonetic(RequestConvertAudioPhoneticDto requestConvert)
        {
            var URL = "http://localhost:8000/transcribe/wav2vec2";

            var jsonPayload = JsonSerializer.Serialize(requestConvert);

            using var request = new HttpRequestMessage(HttpMethod.Post, URL);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException($"Submit to Backend python failed: {content}");
            }

            var submitResult = JsonSerializer.Deserialize<ResponseConvertAudioPhoneticDto>(content)
                ?? throw new BadRequestException($"resul backend python is null: {content}"); ;

            return submitResult;
        }
    }
}
