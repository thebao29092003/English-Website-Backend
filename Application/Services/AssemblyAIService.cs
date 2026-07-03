using English.Website.Api.Dtos.AIDtos.AssemblyAIDto;
using English.Website.Api.Dtos.AIDtos.AzureSpeechDto;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using English.Website.Domain.Constants;
using English.Website.Domain.DatabaseContext;
using English.Website.Domain.Entities.AI.AIModelAudio;
using English.Website.Api.Dtos.AIDtos.DeepSeekDto; 
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace English.Website.Application.Services
{
    public class AssemblyAIService : IAssemblyAIService
    {
        private readonly string _apiKey;
        private readonly string _webhookAuth;
        private readonly string _webhookUrl;
        private readonly EnglishDBContext _englishDBContext;
        private readonly HttpClient _httpClient;
        private readonly IDeepSeekService _deepSeekService;

        public AssemblyAIService(
            IConfiguration configuration,
            HttpClient httpClient,
            EnglishDBContext englishDBContext,
            IDeepSeekService deepSeekService
        )
        {
            _httpClient = httpClient;
            _apiKey = configuration["AI:AssemblyAIKey"]!;
            _webhookAuth = configuration["WebHook:AssemblyAI:Token"]!;
            _webhookUrl = configuration["WebHook:AssemblyAI:Url"]!;
            _englishDBContext = englishDBContext;
            _deepSeekService = deepSeekService;
        }

        public async Task GetDataAssemblyAI(string transcriptId)
        {
            AssemblyAIResponseDto assemblyAiResult = await CallAPIGetDataAssemblyAI(transcriptId);

            var speechToText = await _englishDBContext.AISpeechToText
                .FirstOrDefaultAsync(s => s.AssemblyAIId == transcriptId)
                ?? throw new BadRequestException($"AISpeechToText record with AssemblyAiId '{transcriptId}' was not found");

            var words = assemblyAiResult.Words;
            var audioDuration = assemblyAiResult.AudioDuration;
            var transcriptResult = assemblyAiResult.Text;

            double wpm = (words?.Count ?? 0) / ((audioDuration ?? 0) / 60.0);

            var fluencyScore = CalculateFluencyScore(words, audioDuration);

            speechToText.AITranscript = transcriptResult;
            speechToText.FluencyScore = fluencyScore; 
            speechToText.OverallConfidence = assemblyAiResult.Confidence ?? 0.0;
            speechToText.WordPerMinute = (int)Math.Round(wpm);
            speechToText.WordsJson = JsonSerializer.Serialize(words);

            speechToText.AudioUsage = new AudioUsage
            {
                UserId = speechToText.UserId,
                AIModelAudioId = 1,
                CalculatedCost = (0.15m / 3600m) * (decimal)(audioDuration ?? 0)
            };

            await _englishDBContext.SaveChangesAsync();

            if (speechToText.TypeAnalyse != TypeAnalyse.NOT && transcriptResult != null)
            {
                var result = await _deepSeekService.CallDeepSeekApi(new TranscriptRequestDto
                {
                    Type = speechToText.TypeAnalyse,
                    UserPrompt = transcriptResult,
                    AISpeechToTextId = speechToText.AISpeechToTextId,
                    UserId = speechToText.UserId,
                });
            }
        }

        public async Task<AssemblyAIResponseDto> CallAPIGetDataAssemblyAI(string transcriptId)
        {
            var headers = new Dictionary<string, string> { { "Authorization", _apiKey } };

            // Khởi tạo request GET lên endpoint của AssemblyAI
            var requestUrl = $"https://api.assemblyai.com/v2/transcript/{transcriptId}";

            var assemblyAiResult = await HttpHelper.SendGetAsync<AssemblyAIResponseDto>(
                  _httpClient,
                  requestUrl,
                  headers
            ) ?? throw new BadRequestException("assemblyAiResult is null");
            return assemblyAiResult;
        }

        public async Task<string> SubmitAudioAssemblyAI(AssemblyAIRequestDto requestDto)
        {

            var headers = new Dictionary<string, string> { { "Authorization", _apiKey } };

            // Khởi tạo request GET lên endpoint của AssemblyAI
            var requestUrl = "https://api.assemblyai.com/v2/transcript";

            requestDto.WebhookAuthHeaderName = "X-Webhook-Secret";
            requestDto.WebhookAuthHeaderValue = _webhookAuth;
            // sau này thay ngrok bằng domain
            requestDto.WebhookUrl = _webhookUrl;

            var assemblyAiResult = await HttpHelper.SendPostJsonAsync<AssemblyAIRequestDto, AssemblyAIResponseDto>(
                 _httpClient,
                 requestUrl,
                 requestDto,
                 headers
            );

            // Trả về Transcript ID ngay lập tức
            return assemblyAiResult.Id;
        }

        public double CalculateFluencyScore(List<AssemblyAIWordDto>? words, double? audioDuration)
        {
            // Điều kiện bảo vệ: Nếu bài nói quá ngắn hoặc rỗng, trả về điểm tối thiểu
            if (words == null || words.Count < 3 || audioDuration <= 0)
            {
                return 10.0; // Điểm sàn tối thiểu
            }

            // 1. TÍNH TỐC ĐỘ NÓI (WPM)
            double durationInMinutes = (audioDuration ?? 0) / 60.0;
            double wpm = words.Count / durationInMinutes;

            double baseScore = 100.0;
            if (wpm < 120.0)
            {
                // Phạt nếu nói quá chậm (mỗi WPM thiếu so với mốc 110 trừ 0.6 điểm)
                baseScore = 100.0 - ((110.0 - wpm) * 0.6);
            }
            else if (wpm > 150.0)
            {
                // Phạt nhẹ nếu nói quá nhanh (mỗi WPM thừa so với mốc 150 trừ 0.4 điểm)
                baseScore = 100.0 - ((wpm - 150.0) * 0.4);
            }

            // Đảm bảo điểm nền không bị âm
            baseScore = Math.Max(40.0, baseScore);


            // 2. PHÂN TÍCH KHOẢNG LẶNG NGẬP NGỪNG (PAUSES)
            double totalDeduction = 0.0;

            for (int i = 0; i < words.Count - 1; i++)
            {
                var currentWord = words[i];
                var nextWord = words[i + 1];

                // Khoảng lặng tính bằng giây (start và end từ AssemblyAI là miliseconds)
                double gapInSeconds = (nextWord.Start - currentWord.End) / 1000.0;

                // Kiểm tra xem từ hiện tại có kết thúc bằng dấu câu (.,?!) không
                string cleanedText = currentWord.Text.Trim();
                bool isNaturalPause = cleanedText.EndsWith(".") ||
                                      cleanedText.EndsWith(",") ||
                                      cleanedText.EndsWith("?") ||
                                      cleanedText.EndsWith("!");

                if (isNaturalPause)
                {
                    // Khoảng ngắt nghỉ tự nhiên: Chỉ trừ điểm nếu im lặng quá lâu (trên 1.8 giây)
                    if (gapInSeconds > 1.8)
                    {
                        totalDeduction += 3.0; // Trừ nhẹ 3 điểm vì ngắt câu quá lâu
                    }
                }
                else
                {
                    // Khoảng lặng ngập ngừng giữa câu (Bí từ, quên ý)
                    if (gapInSeconds > 1.2)
                    {
                        totalDeduction += 6.0; // Ngập ngừng nặng: Trừ 6 điểm
                    }
                    else if (gapInSeconds > 0.8)
                    {
                        totalDeduction += 3.0; // Ngập ngừng nhẹ: Trừ 3 điểm
                    }
                }
            }

            // 3. TỔNG HỢP ĐIỂM SỐ CUỐI CÙNG
            double finalScore = baseScore - totalDeduction;

            // Giới hạn điểm số nằm trong thang điểm từ 10 đến 100
            finalScore = Math.Clamp(finalScore, 10.0, 100.0);

            return Math.Round(finalScore, 1);
        }
    }
}
