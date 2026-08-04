namespace English.Website.Api.Dtos.StatisticDtos
{
    public class UserAverageScoreDto
    {
        public double AveragePronunciationScore { get; set; }
        public double AverageFluencyScore { get; set; }
        public double AverageOverallConfidence { get; set; }
        public double AverageGrammarScore { get; set; }
        public double AverageVocabScore { get; set; }
        public double OverallAverageScore { get; set; }
        public int TotalRecordings { get; set; }
        public double TotalDuration { get; set; }
        public int CurrentStreak { get; set; }
        public int WeeklyRecordingsDiff { get; set; }
    }
}
