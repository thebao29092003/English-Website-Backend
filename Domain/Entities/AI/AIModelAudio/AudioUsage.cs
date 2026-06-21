using English.Website.Domain.Entities.AI.AIModelText;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace English.Website.Domain.Entities.AI.AIModelAudio
{
    public class AudioUsage
    {
        [Key]
        public Guid AudioUsageId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        // ĐÃ SỬA: Bỏ [Required] và chuyển sang Guid? để giữ lại log tiền khi xóa file âm thanh
        public int AIModelAudioId { get; set; }

        [Required]
        public Guid AISpeechToTextId { get; set; }
        
        [Column(TypeName = "decimal(18, 8)")]
        public decimal CalculatedCost { get; set; } // Chi phí thực tế của lượt gọi STT này
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(AIModelAudioId))]
        public AIModelAudio AIModelAudio { get; set; } = null!;

        // vì AudioUsage có sau AISpeechToText nên nó phải có khóa ngoại để tham chiếu
        [ForeignKey(nameof(AISpeechToTextId))]
        public AISpeechToText? AISpeechToText { get; set; }
    }
}
