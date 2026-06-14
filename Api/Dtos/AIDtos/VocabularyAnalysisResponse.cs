using System.Text.Json.Serialization;

namespace English.Website.Api.Dtos.AIDtos
{
    public class VocabularyAnalysisResponse
    {
        [JsonPropertyName("vocabularyAnalysis")]
        public VocabularyAnalysisDetail VocabularyAnalysis { get; set; } = new();
    }

    public class VocabularyAnalysisDetail
    {
        [JsonPropertyName("overallVocabScore")]
        public int OverallVocabScore { get; set; }

        [JsonPropertyName("suggestions")]
        public List<VocabularySuggestionDetail> Suggestions { get; set; } = [];
    }

    public class VocabularySuggestionDetail
    {
        [JsonPropertyName("originalWord")]
        public string OriginalWord { get; set; } = string.Empty;

        [JsonPropertyName("suggestedAlternative")]
        public string SuggestedAlternative { get; set; } = string.Empty;

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;
    }
}
