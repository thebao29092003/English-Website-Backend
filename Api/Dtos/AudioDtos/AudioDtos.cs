using System;
using System.Text.Json.Nodes;
using English.Website.Domain.Constants;

namespace English.Website.Api.Dtos.AudioDtos
{
    public class AudioRecordingDto
    {
        public Guid RecodingId { get; set; }
        public string FileName { get; set; } = null!;
        public long FileSize { get; set; }
        public string FileType { get; set; } = null!;
        public string? FileUrl { get; set; }
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

    public class AudioDetailDto
    {
        public Guid RecodingId { get; set; }
        public string FileName { get; set; } = null!;
        public long FileSize { get; set; }
        public string FileType { get; set; } = null!;
        public string? FileUrl { get; set; }
        public double Duration { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? AITranscript { get; set; }
        public double? OverallConfidence { get; set; }
        public JsonNode? WordsJson { get; set; }
        public double? FluencyScore { get; set; }
        public double? PronunciationScore { get; set; }
        public JsonNode? WordsPronunciationScore { get; set; }
        public JsonNode? FluencyErrors { get; set; }
        public int? WordPerMinute { get; set; }
        public TypeAnalyse? TypeAnalyse { get; set; }
        public JsonNode? AnalysisContentJson { get; set; }
        public double? OverallGrammarScore { get; set; }
        public double? OverallVocabScore { get; set; }
    }
}
