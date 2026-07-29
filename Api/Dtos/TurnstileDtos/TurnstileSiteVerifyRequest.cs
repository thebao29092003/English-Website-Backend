using System.Text.Json.Serialization;

namespace English.Website.Api.Dtos.TurnstileDtos
{
    public class TurnstileSiteVerifyRequest
    {
        [JsonPropertyName("secret")]
        public string Secret { get; set; } = string.Empty;

        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        [JsonPropertyName("remoteip")]
        public string? RemoteIp { get; set; }
    }
}
