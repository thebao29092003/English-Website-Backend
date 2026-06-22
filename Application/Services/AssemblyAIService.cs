using AutoMapper;
using English.Website.Api.Dtos.AIDtos.AssemblyAIDto;
using English.Website.Api.Dtos.AIDtos.AzureSpeechDto;
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
    public class AssemblyAIService : IAssemblyAIService
    {
        private readonly string _apiKey;
        private readonly string _webhookAuth;
        private readonly EnglishDBContext _englishDBContext;
        private readonly HttpClient _httpClient;
        private readonly IUserContextService _userContextService;

        public AssemblyAIService(
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

        public Task<AssemblyAIResponseDto> GetTranscriptStatusAsync(string transcriptId)
        {
            throw new NotImplementedException();
        }

        public Task<AssemblyAIResponseDto> PollUntilCompletedAsync(string transcriptId)
        {
            throw new NotImplementedException();
        }

        public async Task<string> SubmitAudio(AssemblyAIRequestDto requestDto)
        {
            requestDto.WebhookAuthHeaderName = "X-Webhook-Secret";
            requestDto.WebhookAuthHeaderValue = _webhookAuth;
            // sau này thay ngrok bằng domain
            //requestDto.WebhookUrl = "https://stephenie-unreversed-christina.ngrok-free.dev/api/assembly/webhook";
            requestDto.WebhookUrl = "https://d7792j24-7025.asse.devtunnels.ms/api/assembly/webhook";

            var jsonPayload = JsonSerializer.Serialize(requestDto);

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.assemblyai.com/v2/transcript");
            request.Headers.Add("Authorization", _apiKey);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");


            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException($"Submit to AssemblyAI failed: {content}");
            }

            var submitResult = JsonSerializer.Deserialize<AssemblyAIResponseDto>(content);
            if (submitResult == null || string.IsNullOrEmpty(submitResult.Id))
            {
                throw new Exception("Failed to retrieve Transcript ID from AssemblyAI.");
            }

            // Trả về Transcript ID ngay lập tức
            return submitResult.Id;
        }
    }
}
