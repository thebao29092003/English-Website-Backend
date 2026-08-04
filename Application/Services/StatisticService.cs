using English.Website.Api.Dtos.StatisticDtos;
using English.Website.Api.Extensions.Helpers;
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

        public int CalculateStreak(IEnumerable<DateTime> createdUtcDates)
        {
            // Chuyển đổi mốc thời gian UTC từ Database sang ngày theo múi giờ Việt Nam (UTC+7)
            // ToHashSet để loại bỏ phần tử trùng lập và tìm kiếm nhanh hơn khi dùng contains
            var vnDates = createdUtcDates
                .Select(utcDate => utcDate.AddHours(7).Date)
                .ToHashSet();

            if (!vnDates.Any())
            {
                return 0;
            }

            var todayVn = DateTime.UtcNow.AddHours(7).Date;

            // Nếu hôm nay đã ghi âm, bắt đầu tính từ hôm nay.
            // Nếu hôm nay chưa ghi âm nhưng hôm qua đã ghi âm, streak vẫn duy trì và tính từ hôm qua.
            DateTime checkDate;
            if (vnDates.Contains(todayVn))
            {
                checkDate = todayVn;
            }
            else if (vnDates.Contains(todayVn.AddDays(-1)))
            {
                checkDate = todayVn.AddDays(-1);
            }
            else
            {
                return 0; // Đã đứt chuỗi (hôm nay và hôm qua đều không ghi âm)
            }

            int streak = 0;
            while (vnDates.Contains(checkDate))
            {
                streak++;
                checkDate = checkDate.AddDays(-1);
            }

            return streak;
        }

        public int CalculateWeeklyDiff(IEnumerable<DateTime> createdUtcDates)
        {
            var todayVn = DateTime.UtcNow.AddHours(7).Date;

            // Xác định Thứ 2 của tuần này theo múi giờ Việt Nam (tuần bắt đầu từ Thứ 2)
            int daysFromMonday = ((int)todayVn.DayOfWeek + 6) % 7;
            var startOfThisWeekVn = todayVn.AddDays(-daysFromMonday);
            var startOfLastWeekVn = startOfThisWeekVn.AddDays(-7);

            int thisWeekCount = 0;
            int lastWeekCount = 0;

            foreach (var utcDate in createdUtcDates)
            {
                var vnDate = utcDate.AddHours(7);
                if (vnDate >= startOfThisWeekVn && vnDate < startOfThisWeekVn.AddDays(7))
                {
                    thisWeekCount++;
                }
                else if (vnDate >= startOfLastWeekVn && vnDate < startOfThisWeekVn)
                {
                    lastWeekCount++;
                }
            }

            return thisWeekCount - lastWeekCount;
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
                    r.CreatedAt,
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
            int currentStreak = CalculateStreak(recordingsData.Select(x => x.CreatedAt));
            int weeklyRecordingsDiff = CalculateWeeklyDiff(recordingsData.Select(x => x.CreatedAt));

            return new UserAverageScoreDto
            {
                AveragePronunciationScore = Math.Round(avgPronunciation),
                AverageFluencyScore = Math.Round (avgFluency),
                AverageOverallConfidence = Math.Round(avgConfidence),
                AverageGrammarScore = Math.Round(avgGrammar),
                AverageVocabScore = Math.Round(avgVocab),
                OverallAverageScore = activeAvgs.Any() ? Math.Round(activeAvgs.Average()) : 0,
                TotalRecordings = recordingsData.Count,
                TotalDuration = Math.Round(totalDuration),
                CurrentStreak = currentStreak,
                WeeklyRecordingsDiff = weeklyRecordingsDiff
            };
        }

        public async Task<List<DailyScoreDto>> GetDailyScoresAsync(DateOnly fromDate, DateOnly toDate)
        {
            var userDetail = await _userContextService.GetUserDetail();
            var userId = userDetail.UserId;

            var start = fromDate;
            var end = toDate;

            var startDateTime = start.ToDateTime(TimeOnly.MinValue);
            var endDateTime = end.ToDateTime(TimeOnly.MinValue);

            var startUtc = startDateTime.AddHours(-7);
            var endUtc = endDateTime.AddDays(1).AddHours(-7);

            var recordingsData = await _dbContext.Recording
                .Where(r => r.UserId == userId && !r.IsDeleted && r.CreatedAt >= startUtc && r.CreatedAt < endUtc)
                .Select(r => new
                {
                    r.RecordingId,
                    r.CreatedAt,
                    VnDate = DateOnly.FromDateTime(r.CreatedAt.AddHours(7)),
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

            var groupedByDate = recordingsData
                .GroupBy(x => x.VnDate)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<DailyScoreDto>();

            for (var current = start; current <= end; current = current.AddDays(1))
            {
                if (groupedByDate.TryGetValue(current, out var dayRecordings))
                {
                    double avgPronunciation = GetAvg(dayRecordings.Select(x => x.PronunciationScore));
                    double avgFluency = GetAvg(dayRecordings.Select(x => x.FluencyScore));
                    double avgConfidence = GetAvg(dayRecordings.Select(x => x.OverallConfidence));
                    double avgGrammar = GetAvg(dayRecordings.Select(x => x.OverallGrammarScore));
                    double avgVocab = GetAvg(dayRecordings.Select(x => x.OverallVocabScore));

                    var activeAvgs = new[] { avgPronunciation, avgFluency, avgConfidence, avgGrammar, avgVocab }
                        .Where(v => v > 0);

                    result.Add(new DailyScoreDto
                    {
                        Date = current,
                        DateString = current.ToString("dd/MM"),
                        AveragePronunciationScore = Math.Round(avgPronunciation),
                        AverageFluencyScore = Math.Round(avgFluency),
                        AverageOverallConfidence = Math.Round(avgConfidence),
                        AverageGrammarScore = Math.Round(avgGrammar),
                        AverageVocabScore = Math.Round(avgVocab),
                        OverallAverageScore = activeAvgs.Any() ? Math.Round(activeAvgs.Average()) : 0
                    });
                }
                else
                {
                    result.Add(new DailyScoreDto
                    {
                        Date = current,
                        DateString = current.ToString("dd/MM"),
                        AveragePronunciationScore = 0,
                        AverageFluencyScore = 0,
                        AverageOverallConfidence = 0,
                        AverageGrammarScore = 0,
                        AverageVocabScore = 0,
                        OverallAverageScore = 0
                    });
                }
            }

            return result;
        }
    }
}
