using System.Text.Json.Serialization;

namespace English.Website.Api.Dtos.BackendPythonDtos
{
    public class PythonPhonemeWebhookDto
    {
        [JsonPropertyName("recordingId")]
        public string RecordingId { get; set; }

        [JsonPropertyName("phonemes")]
        public string Phonemes { get; set; } = string.Empty;
    }
}
