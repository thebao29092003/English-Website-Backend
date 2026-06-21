using System.Text.Json.Serialization;

namespace English.Website.Api.Dtos.AIDtos.DeepSeekDto
{
    // Cấu trúc Response nhận về từ DeepSeek
    public class DeepSeekResponseDto
    {
        [JsonPropertyName("choices")]
        public List<DeepSeekChoice>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public DeepSeekUsageDto? Usage { get; set; }
    }

    public class DeepSeekMessage
    {
        [JsonPropertyName("role")]
        public required string Role { get; set; }

        [JsonPropertyName("content")]
        public required string Content { get; set; }
    }


    public class DeepSeekChoice
    {
        [JsonPropertyName("message")]
        public DeepSeekMessage? Message { get; set; }
    }

    public class DeepSeekUsageDto
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }

        // thuộc tính này dưới với PromptCacheHitTokens là 1 
        // nhưng cái dưới dùng để tương thích với sdk của openAI thôi
        [JsonPropertyName("prompt_tokens_details")]
        public PromptTokensDetails? PromptTokensDetails { get; set; }

        [JsonPropertyName("completion_tokens_details")]
        public CompletionTokensDetails? CompletionTokensDetails { get; set; }

        [JsonPropertyName("prompt_cache_hit_tokens")]
        public int PromptCacheHitTokens { get; set; }

        [JsonPropertyName("prompt_cache_miss_tokens")]
        public int PromptCacheMissTokens { get; set; }
    }

    public class PromptTokensDetails
    {
        [JsonPropertyName("cached_tokens")]
        public int CachedTokens { get; set; }
    }

    public class CompletionTokensDetails
    {
        [JsonPropertyName("reasoning_tokens")]
        public int ReasoningTokens { get; set; }
    }

    // Lớp bọc kết quả trả về chứa cả dữ liệu dạng Object và lượng Token đã tiêu thụ
    public class DeepSeekResult<T>
    {
        public T? Data { get; set; }
        public DeepSeekUsageDto? Usage { get; set; }
    }
}