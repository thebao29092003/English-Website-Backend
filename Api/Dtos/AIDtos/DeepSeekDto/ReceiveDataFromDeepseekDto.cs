using System.Text.Json.Serialization;

namespace English.Website.Api.Dtos.AIDtos.DeepSeekDto
{
    public class ReceiveDataFromDeepseekDto
    {
        // Trỏ thẳng đến lớp Detail, không trỏ đến lớp Response bọc ngoài
        [JsonPropertyName("grammarAnalysis")]
        public GrammarAnalysisDetail? GrammarAnalysis { get; set; }

        [JsonPropertyName("vocabularyAnalysis")]
        public VocabularyAnalysisDetail? VocabularyAnalysis { get; set; } 

        // Trỏ thẳng đến danh sách List luôn
        [JsonPropertyName("rephrasedResponses")]
        public List<RephrasedResponseDetail>? RephrasedResponses { get; set; }

        [JsonPropertyName("toeicEvaluation")]
        public ToeicEvaluationDetail? ToeicEvaluation { get; set; }
    }
}
