using CloudinaryDotNet.Actions;
using English.Website.Api.Dtos.AIDtos.AssemblyAIDto;
using English.Website.Api.Dtos.AIDtos.AzureSpeechDto;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using System.Text;
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

        public async Task GetDataAssemblyAI(string transcriptId)
        {
            // Khởi tạo request GET lên endpoint của AssemblyAI
            var requestUrl = $"https://api.assemblyai.com/v2/transcript/{transcriptId}";
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", _apiKey);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new BadRequestException($"Failed to get transcript from AssemblyAI. Status: {response.StatusCode}, Content: {content}");
            }

            // Deserialize kết quả JSON thô thu được về Object DTO
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var assemblyAiResult = JsonSerializer.Deserialize<AssemblyAIResponseDto>(content, jsonOptions)
                ?? throw new BadRequestException("Failed to deserialize AssemblyAI response.");

            var speechToText = await _englishDBContext.AISpeechToText
                .FirstOrDefaultAsync(s => s.AssemblyAIId == transcriptId)
                ?? throw new BadRequestException($"AISpeechToText record with AssemblyAiId '{transcriptId}' was not found");

            var fluencyScore = CalculateFluencyScore(assemblyAiResult.Words!, assemblyAiResult.AudioDuration);
           
            speechToText.AITranscript = assemblyAiResult.Text;
            speechToText.FluencyScore = fluencyScore;
            speechToText.OverallConfidence = assemblyAiResult.Confidence ?? 0.0;
            speechToText.WordsJson = JsonSerializer.Serialize(assemblyAiResult.Words);

            await _englishDBContext.SaveChangesAsync();
        }

        public async Task<AssemblyAIResponseDto> CallAPIDeepSeek(string transcriptId)
        {
            return null;

        }


        public async Task<string> SubmitAudioAssemblyAI(AssemblyAIRequestDto requestDto)
        {
            requestDto.WebhookAuthHeaderName = "X-Webhook-Secret";
            requestDto.WebhookAuthHeaderValue = _webhookAuth;
            // sau này thay ngrok bằng domain
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

        public double CalculateFluencyScore(List<AssemblyAIWordDto> words, double? audioDuration)
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
            if (wpm < 110.0)
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
