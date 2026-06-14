using AutoMapper;
using English.Website.Api.Dtos.AIDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Extend;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using English.Website.Domain.Entities.AI;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace English.Website.Application.Services
{
    public class DeepSeekService : IDeepSeekService
    {
        private readonly string _apiKey;
        private readonly EnglishDBContext _englishDBContext;
        private readonly HttpClient _httpClient;
        private readonly IUserContextService _userContextService;

        public DeepSeekService(
            IConfiguration configuration,
            HttpClient httpClient,
            IUserContextService userContextService,
            EnglishDBContext englishDBContext
        )
        {
            _httpClient = httpClient;
            _apiKey = configuration["AI:DeepSeekApiKey"]!;
            _userContextService = userContextService;
            _englishDBContext = englishDBContext;
        }

        private HttpRequestMessage PrepareCallApi(string systemPrompt, string userPrompt, Guid userId)
        {
            var requestUrl = "https://api.deepseek.com/chat/completions";

            var requestBody = new DeepSeekRequestDto
            {
                Model = "deepseek-v4-flash", // Hoặc model tương ứng của bạn
                Messages = new List<DeepSeekMessage>
            {
                new DeepSeekMessage { Role = "system", Content = systemPrompt },
                new DeepSeekMessage { Role = "user", Content = userPrompt }
            },
                ResponseFormat = new DeepSeekResponseFormat { Type = "json_object" },
                Temperature = 0.2,
                UserId = userId,
                Thinking = new DeepSeekThingKingMode { Type = "disabled" },
                MaxTokens = 2048
            };
            // Chuyển từ object sang string JSON
            var jsonPayload = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            return request;
        }

        // Generic Method: khi gọi phải truyền 1 kiểu cụ thể vào <>
        // method này trả về  về một Task bất đồng bộ, khi hoàn thành nó sẽ sinh ra một đối tượng DeepSeekResult<T>
        // có kiểu dữ liệu tương ứng với kiểu T mà ta đã truyền vào khi gọi hàm.
        public async Task<DeepSeekResult<T>> CallApiAsync<T>(string systemPrompt, string userPrompt, Guid userId)
        {
            using HttpRequestMessage request = PrepareCallApi(systemPrompt, userPrompt, userId);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException($"DeepSeek API request failed with status code {response.StatusCode}: {content}");
            }

            var result = JsonSerializer.Deserialize<DeepSeekResponseDto>(content);
            var messageContent = result?.Choices?.FirstOrDefault()?.Message?.Content;
            var usage = result?.Usage;

            if (string.IsNullOrEmpty(messageContent))
            {
                throw new InvalidOperationException("DeepSeek returned an empty message content.");
            }

            // Cấu hình Deserialize không phân biệt chữ hoa chữ thường để khớp trường từ JSON của AI trả về
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Ép kiểu chuỗi JSON thô thu được từ DeepSeek sang định dạng Object kiểu T
            T? data = JsonSerializer.Deserialize<T>(messageContent, jsonOptions);

            return new DeepSeekResult<T>
            {
                Data = data,
                Usage = usage
            };

        }

        public async Task<ReceiveDataFromDeepseekDto> CallDeepSeekApi(TranscriptRequestDto deepSeekRequest)
        {
            string systemPrompt = SystemPrompt.systemPromptGeneric;
            string userPrompt = deepSeekRequest.userPrompt;

            var user = await _userContextService.GetUserDetail();
            var userId = user?.UserId ?? throw new BadRequestException("UserId not found"); ;

            string systemPromptGramma = $"{systemPrompt}\n{SystemPrompt.systemPromptGrammar}";
            string systemPromptVocab = $"{systemPrompt}\n{SystemPrompt.systemPromptVocab}";

            var grammarTask = CallApiAsync<GrammarAnalysisResponse>(systemPromptGramma, userPrompt, userId);
            var vocabTask = CallApiAsync<VocabularyAnalysisResponse>(systemPromptVocab, userPrompt, userId);

            // Khai báo trước các Task tùy chọn (cho chế độ FULL) ở phạm vi ngoài if
            Task<DeepSeekResult<RephrasedResponsesResponse>>? rephraseTask = null;
            Task<DeepSeekResult<ToeicEvaluationResponse>>? evaluationTask = null;

            // Khởi tạo danh sách các Task cần đợi
            var tasksToAwait = new List<Task> { grammarTask, vocabTask };

            // 3. Nếu là chế độ FULL, kích hoạt thêm các Task bổ sung và đưa vào danh sách chờ
            bool isFull = deepSeekRequest.type == "FULL";
            if (isFull)
            {
                string systemPromptRephrase = $"{systemPrompt}\n{SystemPrompt.systemPromptRephrasing}";
                string systemPromptFeedback = $"{systemPrompt}\n{SystemPrompt.systemPromptDetailed}";

                rephraseTask = CallApiAsync<RephrasedResponsesResponse>(systemPromptRephrase, userPrompt, userId);
                evaluationTask = CallApiAsync<ToeicEvaluationResponse>(systemPromptFeedback, userPrompt, userId);

                tasksToAwait.Add(rephraseTask);
                tasksToAwait.Add(evaluationTask);
            }

            await Task.WhenAll(tasksToAwait);

            // Lấy kết quả từ các task
            var grammarResult = grammarTask.Result;
            var vocabResult = vocabTask.Result;
            var rephraseResult = isFull ? rephraseTask?.Result : null;
            var feedbackResult = isFull ? evaluationTask?.Result : null;

            var mergedResultDto = new ReceiveDataFromDeepseekDto
            {
                GrammarAnalysis = grammarResult.Data?.GrammarAnalysis,
                VocabularyAnalysis = vocabResult.Data?.VocabularyAnalysis,
                RephrasedResponses = rephraseResult?.Data?.RephrasedResponses,
                ToeicEvaluation = feedbackResult?.Data?.ToeicEvaluation
            };

            // 5. GOM NHÓM VÀ CỘNG DỒN TOKEN USAGE
            // Thu thập các đối tượng Usage không bị null từ các task đã chạy
            var usages = new List<DeepSeekUsageDto>();
            if (grammarResult?.Usage != null) usages.Add(grammarResult.Usage);
            if (vocabResult?.Usage != null) usages.Add(vocabResult.Usage);
            if (rephraseResult?.Usage != null) usages.Add(rephraseResult.Usage);
            if (feedbackResult?.Usage != null) usages.Add(feedbackResult.Usage);

            // Tính toán cộng dồn
            int totalTokens = usages.Sum(u => u.TotalTokens);
            int promptTokens = usages.Sum(u => u.PromptTokens);
            int cacheMissTokens = usages.Sum(u => u.PromptCacheMissTokens);
            int cacheHitTokens = usages.Sum(u => u.PromptCacheHitTokens);
            int completionTokens = usages.Sum(u => u.CompletionTokens);
            int reasoningTokens = usages.Sum(u => u.CompletionTokensDetails?.ReasoningTokens ?? 0);


            // 6. THỰC THI TRANSACTION LƯU DATABASE
            using var transaction = await _englishDBContext.Database.BeginTransactionAsync();
            try
            {
                // Khởi tạo bản ghi TokenUsage tổng
                TokenUsage tokenUsage = new TokenUsage
                {
                    AIModelTextId = 1, // Model ID của bạn
                    UserId = userId,
                    TotalTokens = totalTokens,
                    PromptTokens = promptTokens,
                    CacheMissTokens = cacheMissTokens,
                    CacheHitTokens = cacheHitTokens,
                    CompletionTokens = completionTokens,
                    ReasoningTokens = reasoningTokens,

                    // Tính tổng chi phí dựa trên tổng token đã gom nhóm
                    CalculatedCost = ((decimal)0.14 * cacheMissTokens +
                                      (decimal)0.0028 * cacheHitTokens +
                                      (decimal)0.28 * completionTokens) / 1000000
                };

                _englishDBContext.TokenUsage.Add(tokenUsage);

                var jsonOptions = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = false // Để false để nén chuỗi gọn nhất có thể giúp tiết kiệm dung lượng DB
                };

                // Chuỗi JSON tổng sau khi gộp để lưu vào AiAnalysis
                // khi lấy ra trả cho user nhớ De
                string mergedJsonContent = JsonSerializer.Serialize(mergedResultDto, jsonOptions);

                AiAnalysis aiAnalysis = new()
                {
                    UserId = userId,
                    TokenUsage = tokenUsage,
                    UserTranscript = userPrompt, // Transcript gốc của user
                    AnalysisContentJson = mergedJsonContent ?? "AI not response"
                };

                _englishDBContext.AiAnalyse.Add(aiAnalysis);

                await _englishDBContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new BadRequestException("error function CallDeepSeekApi");
            }
            return mergedResultDto;
        }
    }
}
