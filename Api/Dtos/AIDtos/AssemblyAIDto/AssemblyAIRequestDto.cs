using System.Text.Json.Serialization;

namespace English.Website.Api.Dtos.AIDtos.AzureSpeechDto
{
    public class AssemblyAIRequestDto

    {
        [JsonPropertyName("audio_url")]
        public string AudioUrl { get; set; } = null!;

        [JsonPropertyName("speech_models")]
        public List<string> SpeechModels { get; set; } = ["universal-2"];

        [JsonPropertyName("format_text")]
        public bool FormatText { get; set; } = true;

        [JsonPropertyName("punctuate")]
        public bool Punctuate { get; set; } = true;

        [JsonPropertyName("language_code")]
        public string LanguageCode { get; set; } = "en";

        // ĐƯỜNG DẪN WEBHOOK CỦA BẠN
        [JsonPropertyName("webhook_url")]
        public string? WebhookUrl { get; set; }

        // Header bảo mật để tránh kẻ xấu spam API Webhook của bạn
        [JsonPropertyName("webhook_auth_header_name")]
        public string? WebhookAuthHeaderName { get; set; }

        [JsonPropertyName("webhook_auth_header_value")]
        public string? WebhookAuthHeaderValue { get; set; }
    }
}
