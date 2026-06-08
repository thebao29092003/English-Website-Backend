using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace English.Website.Domain.Entities.AI
{
    public class AiAnalysis
    {
        [Key]
        public Guid AiAnalysisId { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public Guid TokenUsageId { get; set; } // Khóa ngoại quan hệ 1-1 sang bảng TokenUsage

        [Required]
        public int AIModelTextId { get; set; }

        [Required]
        public string UserTranscript { get; set; } = null!; // Lưu vết đoạn text học sinh nói

        [Required]
        public string AnalysisContentJson { get; set; } = null!; // Lưu chuỗi JSON phân tích (sạch, không có \)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(TokenUsageId))]
        public TokenUsage TokenUsage { get; set; } = null!;
    }
}
