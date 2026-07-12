using System;
using System.Text.Json.Nodes;
using English.Website.Domain.Constants;

namespace English.Website.Api.Dtos.HomeDtos
{
    public class AudioDetailDto
    {
        public Guid RecodingId { get; set; }
        public string FileName { get; set; } = null!;
        public long FileSize { get; set; }
        public string FileType { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
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
