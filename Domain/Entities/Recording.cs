using English.Website.Domain.Entities.AI;
using System.ComponentModel.DataAnnotations;

namespace English.Website.Domain.Entities
{
    public class Recording
    {
        [Key]
        public Guid RecordingId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public string? CloudinaryPublicId { get; set; }

        public string? Url { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        [Required]
        public string FileName { get; set; } = null!; // Tên file gốc (ví dụ: "speaking_task1.wav")

        public long FileSize { get; set; } // Dung lượng file tính bằng bytes

        [Required]
        public string FileType { get; set; } = null!; // Định dạng file (ví dụ: "wav", "mp3")

        public double Duration { get; set; } // Thời lượng file (giây) - Phục vụ tính WPM và tiền STT

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public AISpeechToText? AISpeechToText { get; set; }
    }
}
