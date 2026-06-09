using AutoMapper;
using English.Website.Api.Dtos.AIDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Extend;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using English.Website.Domain.Entities.AI;
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
        private readonly EnglishDBContext _englishDBContext;
        private readonly IMapper _mapper;

        public DeepSeekService(
            IConfiguration configuration,
            HttpClient httpClient,
            IUserContextService userContextService,
            EnglishDBContext englishDBContext,
            IMapper mapper
        )
        {
            _httpClient = httpClient;
            _apiKey = configuration["AI:DeepSeekApiKey"]!;
            _userContextService = userContextService;
            _englishDBContext = englishDBContext;
            _mapper = mapper;
        }

        public async Task<string> AnalyzeSpeech(TranscriptRequestDto deepSeekRequest)
        {
            var requestUrl = "https://api.deepseek.com/chat/completions";
            var user = await _userContextService.GetUserDetail();
            var userId = user?.UserId;

            if(userId == null)
            {
                throw new BadRequestException("UserId not found");
            }

            string systemPrompt = deepSeekRequest.type switch
            {
                "FULL" => SystemPrompt.systemPromptFull, 
                "QUICK" => SystemPrompt.systemPrompt,
                _ => throw new BadRequestException("Invalid type. Must be either 'FULL' or 'QUICK'")
            };



            var requestBody = new DeepSeekRequestDto
            {
                Model = "deepseek-chat",
                Messages = new List<DeepSeekMessage>
                    {
                        new DeepSeekMessage { Role = "system", Content = systemPrompt },
                        new DeepSeekMessage { Role = "user", Content = deepSeekRequest.userPrompt }
                    },
                ResponseFormat = new DeepSeekResponseFormat { Type = "json_object" },
                Temperature = 0.2,

                UserId =  userId,
                
                Thinking = new DeepSeekThingKingMode { Type = "disble" },

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
            var messageContent = result?.Choices?.FirstOrDefault()?.Message?.Content;
            var usage = result?.Usage;

            if (result == null || string.IsNullOrEmpty(messageContent) || usage == null)
            {
                throw new BadRequestException("Deepseek not response");
            }

            var transaction = await _englishDBContext.Database.BeginTransactionAsync();

            try
            {
                TokenUsage tokenUsage = new TokenUsage {

                    AIModelTextId = 1,
                    UserId = (Guid)userId,

                    TotalTokens = usage?.TotalTokens ?? 0,
                    PromptTokens = usage?.PromptTokens ?? 0,


                    CacheMissTokens = usage?.PromptCacheMissTokens,
                    CacheHitTokens = usage?.PromptCacheHitTokens,

                    CompletionTokens = usage?.CompletionTokens ?? 0,

                    ReasoningTokens = usage?.CompletionTokensDetails == null ? 0 : usage?.CompletionTokensDetails.ReasoningTokens ?? 0,

                    CalculatedCost = 
                       ( (decimal)0.14 * (usage?.PromptCacheMissTokens ?? 0) +
                        (decimal)0.0028 * (usage?.PromptCacheHitTokens ?? 0) +
                        (decimal)0.28 * (usage?.CompletionTokens ?? 0) ) / 1000000
                };
                

                _englishDBContext.TokenUsage.Add(tokenUsage);

                AiAnalysis aiAnalysis = new()
                {
                    UserId = (Guid) userId,
                    TokenUsage = tokenUsage,
                    UserTranscript = deepSeekRequest.userPrompt,
                    AnalysisContentJson = messageContent ?? "Ai not response"
                };

                _englishDBContext.AiAnalyse.Add(aiAnalysis);

                await _englishDBContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            // Trả về chuỗi JSON kết quả đã phân tích (chứa grammarAnalysis, vocabularyAnalysis...)
            return messageContent!;
        }
    }
}
