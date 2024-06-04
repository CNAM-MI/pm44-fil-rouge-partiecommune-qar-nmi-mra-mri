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

            builder.Entity<LobbyModel>()
                .ToTable("lobbies");

            builder.Entity<UserModel>()
                .ToTable("users");

            builder.Entity<VoteModel>()
                .ToTable("votes")
                .HasKey(v => new { v.LobbyId, v.UserId, v.OsmId });



            builder.Entity<LobbyModel>()
                .HasMany(lobby => lobby.Users)
                .WithMany(user => user.LobbiesAsUser)
                .UsingEntity("lobby_user");

            builder.Entity<LobbyModel>()
                .HasOne(l => l.Admin)
                .WithMany(u => u.LobbiesAsAdmin)
                .HasForeignKey(l => l.AdminId);

            builder.Entity<VoteModel>()
                .HasOne(v => v.Lobby)
                .WithMany(l => l.Votes)
                .HasForeignKey(v => v.LobbyId);

            builder.Entity<VoteModel>()
                .HasOne(v => v.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(v => v.UserId);
        }

        public DbSet<LobbyModel> Lobbies { get; set; }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<VoteModel> Votes { get; set; }

    }
}
