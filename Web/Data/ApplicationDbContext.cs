using Microsoft.EntityFrameworkCore;
using Web.Models;

namespace Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define your database tables (DbSets)
        public DbSet<Users> Users { get; set; }
        public DbSet<Players> Players { get; set; }
        public DbSet<Teams> Teams { get; set; }
        public DbSet<FantasyTeam> FantasyTeams { get; set; }
        public DbSet<PlayerRoundPoints> PlayerRoundPoints { get; set; }
        public DbSet<UserRoundPoints> UserRoundPoints { get; set; }
        public DbSet<UserRoundTeam> UserRoundTeams { get; set; }
        public DbSet<CurrentRound> CurrentRound { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Players -> Teams relationship
            modelBuilder.Entity<Players>()
                .HasOne(p => p.Team)
                .WithMany(t => t.Players)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure FantasyTeam -> Users relationship
            modelBuilder.Entity<FantasyTeam>()
                .HasOne(ft => ft.User)
                .WithMany()
                .HasForeignKey(ft => ft.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure FantasyTeam -> Players relationship
            modelBuilder.Entity<FantasyTeam>()
                .HasOne(ft => ft.Player)
                .WithMany()
                .HasForeignKey(ft => ft.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure boolean fields for SQLite
            modelBuilder.Entity<FantasyTeam>()
                .Property(ft => ft.IsActive)
                .HasConversion<int>();

            modelBuilder.Entity<FantasyTeam>()
                .Property(ft => ft.IsOnCourt)
                .HasConversion<int>();

            // Configure boolean field for PlayerRoundPoints
            modelBuilder.Entity<PlayerRoundPoints>()
                .Property(prp => prp.TeamWin)
                .HasConversion<int>();

            // Configure PlayerRoundPoints -> Players relationship
            modelBuilder.Entity<PlayerRoundPoints>()
                .HasOne(prp => prp.Player)
                .WithMany()
                .HasForeignKey(prp => prp.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure UserRoundPoints -> Users relationship
            modelBuilder.Entity<UserRoundPoints>()
                .HasOne(urp => urp.User)
                .WithMany()
                .HasForeignKey(urp => urp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure UserRoundTeam -> Users relationship
            modelBuilder.Entity<UserRoundTeam>()
                .HasOne(urt => urt.User)
                .WithMany()
                .HasForeignKey(urt => urt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create unique index for UserId + Round combination
            modelBuilder.Entity<UserRoundTeam>()
                .HasIndex(urt => new { urt.UserId, urt.Round })
                .IsUnique();
        }
    }
}