using System.Text.Json.Serialization;

namespace English.Website.Api.Dtos.AIDtos.AssemblyAIDto
{
    public class AssemblyAiWebhookDto
    {
        [JsonPropertyName("transcript_id")]
        public string TranscriptId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty; // "completed" hoặc "failed"
    }
}
