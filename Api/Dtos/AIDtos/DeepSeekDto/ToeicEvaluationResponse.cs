using System.Text.Json.Serialization;

namespace English.Website.Api.Dtos.AIDtos.DeepSeekDto
{
    public class ToeicEvaluationResponse
    {
        [JsonPropertyName("toeicEvaluation")]
        public ToeicEvaluationDetail ToeicEvaluation { get; set; } = new();
    }

    public class ToeicEvaluationDetail
    {
        [JsonPropertyName("detailedFeedback")]
        public string DetailedFeedback { get; set; } = string.Empty;
    }
}
