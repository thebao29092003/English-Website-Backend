using System.Text.Json.Serialization;

namespace English.Website.Api.Dtos.AIDtos.BackendPythonDto
{
    public class RequestConvertAudioPhoneticDto
    {
        [JsonPropertyName("recording_id")]
        public Guid? RecordingId { get; set; }
       
        [JsonPropertyName("audio_path")]
        public string? AudioPath { get; set; }
        
        [JsonPropertyName("callback_url")]
        public string? CallbackUrl { get; set; }
    }
    public class ResponseConvertAudioPhoneticDto
    {
        public int? statusCode { get; set; }
        public string? message { get; set; }
    }
}
