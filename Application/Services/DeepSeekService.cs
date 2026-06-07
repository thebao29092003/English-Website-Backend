using English.Website.Api.Dtos.AIDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Extend;
using English.Website.Application.Services.IServices;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace English.Website.Application.Services
{
    public class DeepSeekService : IDeepSeekService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IUserContextService _userContextService;

        public DeepSeekService(
            IConfiguration configuration, 
            HttpClient httpClient,
            IUserContextService userContextService)
        {
            _httpClient = httpClient;
            _apiKey = configuration["AI:DeepSeekApiKey"]!;
            _userContextService = userContextService;
        }

        public async Task<DeepSeekResponseDto?> AnalyzeSpeech(TranscriptRequestDto deepSeekRequest)
        {
            var requestUrl = "https://api.deepseek.com/chat/completions";
            var user = await _userContextService.GetUserDetail();
            string userId = user.UserId.ToString();

            string systemPrompt;
            if (deepSeekRequest.type == "FULL")
            {
                systemPrompt = SystemPrompt.systemPropmtFull;
            }
            else if(deepSeekRequest.type == "QUICK")
            {
                systemPrompt = SystemPrompt.systemPrompt;
            } 
            else
            {
                throw new BadRequestException("Invalid type. Must be either 'FULL' or 'QUICK'");
            }
            var requestBody = new DeepSeekRequestDto
            {
                Model = "deepseek-v4-flash",
                Messages = new List<DeepSeekMessage>
                    {
                        new DeepSeekMessage { Role = "system", Content = systemPrompt },
                        new DeepSeekMessage { Role = "user", Content = deepSeekRequest.userPropmt }
                    },
                ResponseFormat = new DeepSeekResponseFormat { Type = "json_object" },
                Temperature = 0.2,

                UserId = !string.IsNullOrEmpty(userId) ? userId : null,

                MaxTokens = 2048
            };

            var jsonPayload = JsonSerializer.Serialize(requestBody);

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException($"DeepSeek API request failed with status code {response.StatusCode}: {content}");
            }

            var result = JsonSerializer.Deserialize<DeepSeekResponseDto>(content);
            //var messageContent = result?.Choices?.FirstOrDefault()?.Message?.Content;
            //var usage = result?.Usage;

            // Trả về chuỗi JSON kết quả đã phân tích (chứa grammarAnalysis, vocabularyAnalysis...)
            return result;
        }
    }
}
