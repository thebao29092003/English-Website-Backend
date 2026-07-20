using English.Website.Api.Dtos.StatisticDtos;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace English.Website.Application.Services
{
    public class StatisticService
    {
        private readonly EnglishDBContext _dbContext;
        private readonly IUserContextService _userContextService;

        public StatisticService(EnglishDBContext dbContext, IUserContextService userContextService)
        {
            _dbContext = dbContext;
            _userContextService = userContextService;
        }

        public double GetAvg(IEnumerable<double?> list)
        {
            // 1. Lọc bỏ các phần tử null và chuyển kiểu dữ liệu từ double? sang double thường
            var valid = list.Where(x => x.HasValue).Select(x => x!.Value);

            /*  nếu bạn gọi hàm .Average() trên một danh sách rỗng (không có phần tử nào),
              hệ thống sẽ ném ra lỗi InvalidOperationException.
              Câu lệnh điều kiện này giúp phòng thủ: nếu không có điểm nào thì trả về 0,
              ngược lại thì tính trung bình và làm tròn tới 2 chữ số thập phân
            */
            // cứ để làm tròn 2 số đi tại tý điểm trung bình của 5 loại điểm
            // nếu làm tròn từng loại điểm trước khi tính trung bình thì gây sai số nhiều
            return valid.Any() ? Math.Round(valid.Average(), 2) : 0;
        }

        public async Task<UserAverageScoreDto> GetUserAverageScores()
        {
            var userDetail = await _userContextService.GetUserDetail();
            var userId = userDetail.UserId;

            var recordingsData = await _dbContext.Recording
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .Select(r => new
                {
                    r.RecordingId,
                    r.Duration,
                    PronunciationScore = r.AISpeechToText != null ? r.AISpeechToText.PronunciationScore * 100 : null,
                    FluencyScore = r.AISpeechToText != null ? r.AISpeechToText.FluencyScore : null,
                    OverallConfidence = r.AISpeechToText != null ? r.AISpeechToText.OverallConfidence * 100 : null,
                    OverallGrammarScore = (r.AISpeechToText != null && r.AISpeechToText.AIAnalysis != null)
                        ? r.AISpeechToText.AIAnalysis.OverallGrammarScore
                        : null,
                    OverallVocabScore = (r.AISpeechToText != null && r.AISpeechToText.AIAnalysis != null)
                        ? r.AISpeechToText.AIAnalysis.OverallVocabScore
                        : null
                })
                .ToListAsync();
            

            double avgPronunciation = GetAvg(recordingsData.Select(x => x.PronunciationScore));
            double avgFluency = GetAvg(recordingsData.Select(x => x.FluencyScore));
            double avgConfidence = GetAvg(recordingsData.Select(x => x.OverallConfidence));
            double avgGrammar = GetAvg(recordingsData.Select(x => x.OverallGrammarScore));
            double avgVocab = GetAvg(recordingsData.Select(x => x.OverallVocabScore));

            var activeAvgs = new[] { avgPronunciation, avgFluency, avgConfidence, avgGrammar, avgVocab }
                .Where(v => v > 0);

            double totalDuration = recordingsData.Sum(x => x.Duration);

            return new UserAverageScoreDto
            {
                AveragePronunciationScore = Math.Round(avgPronunciation),
                AverageFluencyScore = Math.Round (avgFluency),
                AverageOverallConfidence = Math.Round(avgConfidence),
                AverageGrammarScore = Math.Round(avgGrammar),
                AverageVocabScore = Math.Round(avgVocab),
                OverallAverageScore = activeAvgs.Any() ? Math.Round(activeAvgs.Average()) : 0,
                TotalRecordings = Math.Round(recordingsData.Count),
                TotalDuration = Math.Round(totalDuration, 2)
            };
        }
    }
}
