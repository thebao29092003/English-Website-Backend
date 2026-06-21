using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace English.Website.Domain.Entities.AI.AIModelText
{
    public class AIModelText
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AIModelTextId { get; set; }

        [Required]
        [StringLength(100)]
        public string AIName { get; set; } = null!; // Tên hiển thị thân thiện (vd: "DeepSeek V4 Flash")

        [Required]
        [StringLength(50)]
        public string Provider { get; set; } = null!; // "DeepSeek", "OpenAI", "Google"

        /// <summary>
        /// Giá của 1 triệu Input Token (Ví dụ thực tế DeepSeek V4: $0.14)
        /// </summary>
        [Column(TypeName = "decimal(18, 6)")]
        public decimal InputPricePerMillion { get; set; }

        /// <summary>
        /// Giá của 1 triệu Output Token (Ví dụ thực tế DeepSeek V4: $0.28)
        /// </summary>
        [Column(TypeName = "decimal(18, 6)")]
        public decimal OutputPricePerMillion { get; set; }

        /// <summary>
        /// Giá của 1 triệu Cache Hit Token (DeepSeek giảm 90% khi trúng cache)
        /// </summary>
        [Column(TypeName = "decimal(18, 6)")]
        public decimal? CacheHitPricePerMillion { get; set; }

        public int ConcurrencyLimit { get; set; }

        /// <summary>
        /// khi lấy ra chỉ lấy model nào được active
        /// </summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<TokenUsage> TokenUsages { get; set; } = new List<TokenUsage>();
    }
}
