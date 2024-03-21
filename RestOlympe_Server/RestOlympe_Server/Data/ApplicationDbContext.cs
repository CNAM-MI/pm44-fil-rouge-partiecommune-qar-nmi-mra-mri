using Microsoft.EntityFrameworkCore;
using RestOlympe_REST.Models.Entities;

namespace RestOlympe_REST.Data
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
