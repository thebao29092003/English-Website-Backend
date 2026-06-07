using System.Text.Json.Serialization;

namespace English.Website.Api.Dtos.AIDtos
{
    public class TranscriptRequestDto
    {
        public required string userPropmt { get; set; } 
        public required string type { get; set; }
    }

    public class DeepSeekRequestDto
    {
        [JsonPropertyName("model")]
        public required string Model { get; set; }

        [JsonPropertyName("messages")]
        public List<DeepSeekMessage> Messages { get; set; } = new();

        [JsonPropertyName("response_format")]
        public DeepSeekResponseFormat ResponseFormat { get; set; } = new();

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } // Để thấp để đảm bảo trả về đúng định dạng JSON

        // truyền này lên deepseek có nhiều tác dụng
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

    }

    public class DeepSeekResponseFormat
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "json_object"; // Ép DeepSeek trả về JSON chuẩn
    }
}
