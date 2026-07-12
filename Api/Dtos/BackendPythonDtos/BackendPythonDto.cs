using System.Text.Json.Serialization;

namespace English.Website.Api.Dtos.BackendPythonDtos
{
    public class RequestConvertAudioPhoneticDto
    {
        [JsonPropertyName("recording_id")]
        public Guid? RecordingId { get; set; }
       
        [JsonPropertyName("audio_path")]
        public string? AudioPath { get; set; }
        
        [JsonPropertyName("callback_url")]
        public string? CallbackUrl { get; set; }

        [JsonPropertyName("transcript_id")]
        public string? TranscriptId { get; set; }
    }
    public class ResponseConvertAudioPhoneticDto
    {
        public int? statusCode { get; set; }
        public string? message { get; set; }
    }

    public class PhoneticCompareRequestDto
    {
        [JsonPropertyName("word_list")]
        public List<string> WordList { get; set; } = [];

        [JsonPropertyName("phonemes_list")]
        public string PhonemesList { get; set; } = string.Empty;
    }

    public class PhoneticCompareResponseDto
    {
        [JsonPropertyName("word_scores")]
        public List<WordScore> WordScores { get; set; } = [];

        [JsonPropertyName("overall_accuracy")]
        public double OverallAccuracy { get; set; }
    }

    public class WordScore
    {
        [JsonPropertyName("word")]
        public string Word { get; set; } = string.Empty;

        [JsonPropertyName("correct_phones")]
        public int CorrectPhones { get; set; }

        [JsonPropertyName("total_phones")]
        public int TotalPhones { get; set; }

        [JsonPropertyName("accuracy")]
        public double Accuracy { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("original_pronunciation")]
        public string OriginalPronunciation { get; set; } = string.Empty;

        [JsonPropertyName("standard_pronunciation")]
        public string StandardPronunciation { get; set; } = string.Empty;
    }
}
