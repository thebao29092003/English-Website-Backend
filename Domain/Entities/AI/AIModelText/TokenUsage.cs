using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace English.Website.Domain.Entities.AI.AIModelText
{
    public class TokenUsage
    {
        [Key]
        public Guid TokenUsageId { get; set; }

        /// <summary>
        ///  // Khóa ngoại liên kết với User của bạn
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int AIModelTextId { get; set; }

        // ĐÃ SỬA: Bỏ [Required] và chuyển sang Guid? để cho phép giữ lại log tiền khi xóa bài học
        public Guid? AIAnalysisId { get; set; }

        /// <summary>
        /// // Tổng số token tiêu thụ
        /// </summary>
        public int TotalTokens { get; set; }

        /// <summary>
        /// // Lượng token đầu vào gửi đi
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// Số token input được cache (giúp giảm tiền)
        /// </summary>
        public int? CacheHitTokens { get; set; }

        /// <summary>
        /// Số token input không cache (giá tiền bình thường)
        /// </summary>
        public int? CacheMissTokens { get; set; }

        /// <summary>
        /// // Lượng token đầu ra AI sinh ra tính theo output bao gồm cả ReasoningTokens rồi
        /// </summary>
        public int CompletionTokens { get; set; }

        /// <summary>
        /// Số token để AI suy nghĩ trước khi đưa ra câu trả lời
        /// </summary>
        public int ReasoningTokens { get; set; }

        /// <summary>
        /// Số tiền thực tế bạn phải trả cho cuộc gọi này (tính bằng USD)
        /// </summary>
        [Column(TypeName = "decimal(18, 8)")]
        public decimal CalculatedCost { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation Properties, nameof nó sẽ biến thuộc tính thành chuỗi tránh viết sai thôi
        /// Khi bạn đặt thẻ [ForeignKey] nằm trên thì tên truyền vào bắt buộc phải trùng với 
        /// tên của thuộc tính Khóa ngoại nằm ngay trong CÙNG bảng đó 
        /// </summary>
        [ForeignKey(nameof(AIModelTextId))]
        public AIModelText AIModelText { get; set; } = null!;

        // vì TokenUsage có sau AIAnalysis nên nó phải có khóa ngoại để tham chiếu
        [ForeignKey(nameof(AIAnalysisId))]
        public AIAnalysis? AIAnalysis { get; set; }
    }
}
