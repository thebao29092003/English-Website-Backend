using English.Website.Domain.Entities.AI.AIModelAudio;
using English.Website.Domain.Entities.AI.AIModelText;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace English.Website.Domain.Entities.AI
{
    public class AISpeechToText
    {
        [Key]
        public Guid AISpeechToTextId { get; set; }
        public Guid UserId { get; set; }

        [Required]
        public Guid RecordingId { get; set; } // Khóa ngoại trỏ đến thực thể cha là Recording

        public string AssemblyAiId { get; set; } = string.Empty; // Lưu "id": "fa80326b-..." để đối soát khi cần
        public string AITranscript { get; set; } = string.Empty; // "text" - Lưu transcript gốc tại đây

        public double OverallConfidence { get; set; } // "confidence": 0.8949 -> Điểm phát âm tổng quan của mô hình

        public string WordsJson { get; set; } = string.Empty;

        public double FluencyScore { get; set; } // Điểm trôi chảy tự tính
        public double PronunciationScore { get; set; } // Điểm phát âm


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(RecordingId))]
        public Recording Recording { get; set; } = null!; // Điều hướng trỏ về file ghi âm gốc

        public AudioUsage? AudioUsage { get; set; }

        public AIAnalysis? AIAnalysis { get; set; }
    }
}
