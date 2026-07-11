using System;

namespace English.Website.Api.Dtos.HomeDtos
{
    public class HomeRecordingDto
    {
        public Guid RecodingId { get; set; }
        public string FileName { get; set; } = null!;
        public long FileSize { get; set; }
        public string FileType { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public double Duration { get; set; }
        public DateTime CreatedAt { get; set; }

        public AISpeechToTextDto? SpeechToText { get; set; }
        public AIAnalysisDto? Analysis { get; set; }
    }

    public class AISpeechToTextDto
    {
        public string? AITranscript { get; set; }
        public double? OverallConfidence { get; set; }
        public double? FluencyScore { get; set; }
        public double? PronunciationScore { get; set; }
    }

    public class AIAnalysisDto
    {
        public double? OverallGrammarScore { get; set; }
        public double? OverallVocabScore { get; set; }
    }
}
