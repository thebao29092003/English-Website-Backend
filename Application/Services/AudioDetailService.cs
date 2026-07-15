using English.Website.Api.Dtos.HomeDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;

namespace English.Website.Application.Services
{
    public class AudioDetailService
    {
        private readonly EnglishDBContext _dbContext;
        private readonly IUserContextService _userContextService;

        public AudioDetailService(EnglishDBContext dbContext, IUserContextService userContextService)
        {
            _dbContext = dbContext;
            _userContextService = userContextService;
        }

        public async Task<AudioDetailDto> GetAudioDetailAsync(string recodingId)
        {
            // Lấy thông tin user từ UserContext
            var userDetail = await _userContextService.GetUserDetail();
            var userId = userDetail.UserId;

            var recording = await _dbContext.Recording
                .Include(r => r.AISpeechToText)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.RecordingId == Guid.Parse(recodingId))
                ?? throw new BadRequestException("Recording not found.");

            var result = new AudioDetailDto
            {
                RecodingId = recording.RecordingId,
                FileName = recording.FileName,
                FileSize = recording.FileSize,
                FileType = recording.FileType,
                FileUrl = recording.Url,
                Duration = recording.Duration,
                CreatedAt = recording.CreatedAt
            };

            if (recording.AISpeechToText != null)
            {
                var AIAnalysis = await _dbContext.AIAnalysis
                    .FirstOrDefaultAsync(AIAnalysis => AIAnalysis.AISpeechToText == recording.AISpeechToText)
                    ?? throw new BadRequestException("Recording not found.");

                if(AIAnalysis != null)
                {
                    result.OverallGrammarScore = AIAnalysis.OverallGrammarScore;
                    result.OverallVocabScore = AIAnalysis.OverallVocabScore;
                    if (!string.IsNullOrEmpty(AIAnalysis.AnalysisContentJson))
                    {
                        try
                        {
                            result.AnalysisContentJson = JsonNode.Parse(AIAnalysis.AnalysisContentJson);
                        }
                        catch
                        {
                            result.AnalysisContentJson = null;
                        }
                    }
                }
                result.AITranscript = recording.AISpeechToText.AITranscript;
                result.OverallConfidence = recording.AISpeechToText.OverallConfidence;
                result.FluencyScore = recording.AISpeechToText.FluencyScore;
                result.PronunciationScore = recording.AISpeechToText.PronunciationScore;
                result.WordPerMinute = recording.AISpeechToText.WordPerMinute;
                result.TypeAnalyse = recording.AISpeechToText.TypeAnalyse;

                if (!string.IsNullOrEmpty(recording.AISpeechToText.FluencyErrorsJson))
                {
                    try
                    {
                        result.FluencyErrors = JsonNode.Parse(recording.AISpeechToText.FluencyErrorsJson);
                    }
                    catch
                    {
                        result.FluencyErrors = null;
                    }
                }

                if (!string.IsNullOrEmpty(recording.AISpeechToText.WordsJson))
                {
                    try
                    {
                        result.WordsJson = JsonNode.Parse(recording.AISpeechToText.WordsJson);
                    }
                    catch
                    {
                        result.WordsJson = null;
                    }
                }

                if (!string.IsNullOrEmpty(recording.AISpeechToText.WordsPronunciationScore))
                {
                    try
                    {
                        result.WordsPronunciationScore = JsonNode.Parse(recording.AISpeechToText.WordsPronunciationScore);
                    }
                    catch
                    {
                        result.WordsPronunciationScore = null;
                    }
                }
            }

            return result;
        }
    }
}
