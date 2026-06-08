using English.Website.Domain.Entities;
using English.Website.Domain.Entities.AI;
using Microsoft.EntityFrameworkCore;

namespace English.Website.Domain.DatabaseContext
{
    public class EnglishDBContext : DbContext
    {
        public EnglishDBContext(DbContextOptions<EnglishDBContext> options) : base(options)
        {
         
        }
        public DbSet<User> Users { get; set; }
        public DbSet<AIModelText> AIModelTexts { get; set; }
        public DbSet<TokenUsage> TokenUsages { get; set; }
        public DbSet<AiAnalysis> AiAnalyses { get; set; }

        /// <summary>
        /// VỚI QUAN HỆ 1-1 THÌ PHẢI CẤU HÌNH ĐỂ EF CORE BIẾT ĐẶT KHÓA NGOẠI Ở BẢNG NÀO
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AiAnalysis>()
                .HasOne(a => a.TokenUsage)
                .WithOne(t => t.AiAnalysis)
                .HasForeignKey<AiAnalysis>(a => a.TokenUsageId); // Chỉ định khóa ngoại nằm ở bảng AiAnalysis
        }
    }
}
