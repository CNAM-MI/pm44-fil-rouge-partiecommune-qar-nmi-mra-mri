using Microsoft.EntityFrameworkCore;
using RestOlympe_Server.Models.Entities;

namespace RestOlympe_Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MessageModel>()
                .ToTable("messages");
        }

        public DbSet<MessageModel> Messages { get; set; }
    }
}
