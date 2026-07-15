using English.Website.Api.Dtos.HomeDtos;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace English.Website.Application.Services
{
    public class HomeService
    {
        private readonly EnglishDBContext _dbContext;
        private readonly IUserContextService _userContextService;

        public HomeService(EnglishDBContext dbContext, IUserContextService userContextService)
        {
            _dbContext = dbContext;
            _userContextService = userContextService;
        }

        public async Task<List<HomeRecordingDto>> GetUserRecordingsAsync()
        {
            // Lấy thông tin user từ UserContext
            var userDetail = await _userContextService.GetUserDetail();
            var userId = userDetail.UserId;

            // Truy vấn database lấy dữ liệu từ 3 bảng và map sang DTO
            var recordings = await _dbContext.Recording
                .Where(r => r.UserId == userId)
                .Select(r => new HomeRecordingDto
                {
                    RecodingId = r.RecordingId,
                    FileName = r.FileName,
                    FileSize = r.FileSize,
                    FileType = r.FileType,
                    FileUrl = r.Url,
                    Duration = r.Duration,
                    CreatedAt = r.CreatedAt,
                    SpeechToText = r.AISpeechToText == null ? null : new AISpeechToTextDto
                    {
                        AITranscript = r.AISpeechToText.AITranscript,
                        OverallConfidence = r.AISpeechToText.OverallConfidence,
                        FluencyScore = r.AISpeechToText.FluencyScore,

                        PronunciationScore = r.AISpeechToText.PronunciationScore != null
                            ? r.AISpeechToText.PronunciationScore * 100
                            : null
                    },
                    Analysis = (r.AISpeechToText == null || r.AISpeechToText.AIAnalysis == null) ? null : new AIAnalysisDto
                    {
                        OverallGrammarScore = r.AISpeechToText.AIAnalysis.OverallGrammarScore,
                        OverallVocabScore = r.AISpeechToText.AIAnalysis.OverallVocabScore
                    }
                })
                .ToListAsync();

            return recordings;
        }
    }
}
