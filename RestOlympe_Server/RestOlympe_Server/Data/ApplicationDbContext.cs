using Microsoft.EntityFrameworkCore;
using RestOlympe_Server.Models.Entities;
using System.Data;

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

            builder.Entity<LobbyModel>()
                .ToTable("lobbies");

            builder.Entity<UserModel>()
                .ToTable("users");



            builder.Entity<LobbyModel>()
                .HasMany(lobby => lobby.Users)
                .WithMany(user => user.LobbiesAsUser)
                .UsingEntity("lobby_user");

            builder.Entity<LobbyModel>()
                .HasOne(l => l.Admin)
                .WithMany(u => u.LobbiesAsAdmin)
                .HasForeignKey(l => l.AdminId);
        }

        public DbSet<LobbyModel> Lobbies { get; set; }
        public DbSet<UserModel> Users { get; set; }

    }
}
