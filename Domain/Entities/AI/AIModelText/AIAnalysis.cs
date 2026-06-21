using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace English.Website.Domain.Entities.AI.AIModelText
{
    public class AIAnalysis
    {
        [Key]
        public Guid AIAnalysisId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public Guid? AISpeechToTextId { get; set; }

        [Required]
        public string UserTranscript { get; set; } = null!; // Lưu vết đoạn text học sinh nói

        [Required]
        public string AnalysisContentJson { get; set; } = null!; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public TokenUsage? TokenUsage { get; set; }

        [ForeignKey(nameof(AISpeechToTextId))]
        public AISpeechToText? AISpeechToText { get; set; } // Trỏ ngược về thực thể cha
    }
}
