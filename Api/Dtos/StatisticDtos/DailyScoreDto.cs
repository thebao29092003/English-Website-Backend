namespace English.Website.Api.Dtos.StatisticDtos
{
    public class DailyScoreDto
    {
        public DateOnly Date { get; set; }
        public string DateString { get; set; } = string.Empty;
        public double AveragePronunciationScore { get; set; }
        public double AverageFluencyScore { get; set; }
        public double AverageOverallConfidence { get; set; }
        public double AverageGrammarScore { get; set; }
        public double AverageVocabScore { get; set; }
        public double OverallAverageScore { get; set; }
    }
}
