using System.Text.Json.Serialization;

namespace English.Website.Api.Dtos.AIDtos.AssemblyAIDto
{
    public class AssemblyAIResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!; // "queued", "processing", "completed", "error"

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("audio_url")]
        public string AudioUrl { get; set; } = null!;

        [JsonPropertyName("audio_duration")]
        public double? AudioDuration { get; set; } // Thời lượng file (giây) [3.1.7]

        [JsonPropertyName("confidence")]
        public double? Confidence { get; set; } // Độ tự tin tổng thể

        [JsonPropertyName("words")]
        public List<AssemblyAiWordDto>? Words { get; set; } // Danh sách từ chi tiết [3.1.5]

        [JsonPropertyName("error")]
        public string? Error { get; set; } // Chứa thông báo lỗi nếu dịch thất bại
    }
    // 3. DTO mô tả chi tiết từng từ để tính Fluency/Pronunciation sau này [3.1.5]
    public class AssemblyAiWordDto
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = null!;

        [JsonPropertyName("start")]
        public int Start { get; set; } // Miliseconds

        [JsonPropertyName("end")]
        public int End { get; set; } // Miliseconds

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }

}
