using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace English.Website.Domain.Entities.AI.AIModelAudio
{
    public class AIModelAudio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AIModelAudioId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        public string Provider { get; set; } = null!;

        public double PricePerHour { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<AudioUsage> AudioUsages { get; set; } = new List<AudioUsage>();
    }
}
