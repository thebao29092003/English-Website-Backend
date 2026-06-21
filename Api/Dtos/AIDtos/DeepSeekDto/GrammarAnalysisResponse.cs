using System.Text.Json.Serialization;

namespace English.Website.Api.Dtos.AIDtos.DeepSeekDto
{
    public class GrammarAnalysisResponse
    {
        [JsonPropertyName("grammarAnalysis")]
        public GrammarAnalysisDetail GrammarAnalysis { get; set; } = new();
    }

    public class GrammarAnalysisDetail
    {
        [JsonPropertyName("overallGrammarScore")]
        public int OverallGrammarScore { get; set; }

        [JsonPropertyName("errors")]
        public List<GrammarErrorDetail> Errors { get; set; } = [];
    }

    public class GrammarErrorDetail
    {
        [JsonPropertyName("original")]
        public string Original { get; set; } = string.Empty;

        [JsonPropertyName("corrected")]
        public string Corrected { get; set; } = string.Empty;

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;
    }
}
