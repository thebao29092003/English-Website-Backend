using English.Website.Domain.Entities;
using English.Website.Domain.Entities.AI;
using English.Website.Domain.Entities.AI.AIModelAudio;
using English.Website.Domain.Entities.AI.AIModelText;
using Microsoft.EntityFrameworkCore;

namespace English.Website.Domain.DatabaseContext
{
    public class EnglishDBContext : DbContext
    {
        public EnglishDBContext(DbContextOptions<EnglishDBContext> options) : base(options)
        {

        }
        public DbSet<User> User { get; set; }
        public DbSet<Recording> Recording { get; set; }

        public DbSet<AIModelText> AIModelText { get; set; }
        public DbSet<TokenUsage> TokenUsage { get; set; }
        public DbSet<AIAnalysis> AIAnalysis { get; set; }

        public DbSet<AIModelAudio> AIModelAudio { get; set; }
        public DbSet<AudioUsage> AudioUsage { get; set; }
        public DbSet<AISpeechToText> AISpeechToText { get; set; }

        /// <summary>
        /// VỚI QUAN HỆ 1-1 THÌ PHẢI CẤU HÌNH ĐỂ EF CORE BIẾT ĐẶT KHÓA NGOẠI Ở BẢNG NÀO
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. BỔ SUNG: Quan hệ 1-1 giữa Recording và AISpeechToText (Khóa ngoại đặt tại AISpeechToText)
            modelBuilder.Entity<AISpeechToText>()
                .HasOne(s => s.Recording)
                .WithOne(r => r.AISpeechToText)
                .HasForeignKey<AISpeechToText>(s => s.RecordingId)
                .OnDelete(DeleteBehavior.Restrict); // Dùng Restrict để bảo vệ file ghi âm gốc

            // 1. Cầu nối mềm 1-1 giữa Phân tích Text và File ghi âm (Xóa AIAnalysis không làm mất ghi âm gốc)
            modelBuilder.Entity<AIAnalysis>()
                .HasOne(a => a.AISpeechToText)
                .WithOne(t => t.AIAnalysis)
                .HasForeignKey<AIAnalysis>(a => a.AISpeechToTextId) // Chỉ định khóa ngoại nằm ở bảng AiAnalysis
                .OnDelete(DeleteBehavior.Restrict);

            // Khi xóa AIAnalysis, bản ghi TokenUsage vẫn tồn tại trong DB, trường AIAnalysisId tự động set về NULL [1].
            modelBuilder.Entity<TokenUsage>()
                .HasOne(a => a.AIAnalysis)
                .WithOne(t => t.TokenUsage)
                .HasForeignKey<TokenUsage>(t => t.AIAnalysisId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TokenUsage>()
               .HasOne(t => t.AIModelText)
               .WithMany(m => m.TokenUsages)
               .HasForeignKey(t => t.AIModelTextId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AudioUsage>()
               .HasOne(a => a.AISpeechToText)
               .WithOne(t => t.AudioUsage)
               .HasForeignKey<AudioUsage>(t => t.AISpeechToTextId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AudioUsage>()
               .HasOne(t => t.AIModelAudio)
               .WithMany(m => m.AudioUsages)
               .HasForeignKey(t => t.AIModelAudioId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
