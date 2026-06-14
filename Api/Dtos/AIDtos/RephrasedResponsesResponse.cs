using System.Text.Json.Serialization;

namespace English.Website.Api.Dtos.AIDtos
{
    public class RephrasedResponsesResponse
    {
        [JsonPropertyName("rephrasedResponses")]
        public List<RephrasedResponseDetail> RephrasedResponses { get; set; } = [];
    }

    public class RephrasedResponseDetail
    {
        [JsonPropertyName("improvedText")]
        public string ImprovedText { get; set; } = string.Empty;

        [JsonPropertyName("style")]
        public string Style { get; set; } = string.Empty; // "High-Score TOEIC" hoặc "Natural Conversational"

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;
    }
}
