using English.Website.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace English.Website.Domain.DatabaseContext
{
    public class EnglishDBContext : DbContext
    {
        public EnglishDBContext(DbContextOptions<EnglishDBContext> options) : base(options)
        {
         
        }
        public DbSet<User> Users { get; set; }
    }
}
