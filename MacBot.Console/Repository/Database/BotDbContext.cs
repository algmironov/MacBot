using MacBot.ConsoleApp.Models;

using Microsoft.EntityFrameworkCore;

namespace MacBot.ConsoleApp.Repository.Database
{
    public class BotDbContext : DbContext
    {
        private const string _databaseName = "bot.db";
        public DbSet<BotUser> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<InviteCode> InviteCodes { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<SessionCard> SessionCards { get; set; }
        public DbSet<BotMessage> BotMessages { get; set; }
        public DbSet<SessionParameters> SessionParameters { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_databaseName}");
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация модели BotUser
            modelBuilder.Entity<BotUser>()
                .HasKey(b => b.Id);

            modelBuilder.Entity<BotUser>()
                .HasMany(b => b.Sessions)
                .WithOne(s => s.Master)
                .HasForeignKey(s => s.MasterId);

            // Конфигурация модели Session
            modelBuilder.Entity<Session>()
                .HasKey(s => s.SessionId);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Master)
                .WithMany(b => b.Sessions)
                .HasForeignKey(s => s.MasterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Session>()
                .HasOne(s => s.Client)
                .WithMany()
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Session>()
            .Property(s => s.IsActive);

            // Конфигурация модели InviteCode
            modelBuilder.Entity<InviteCode>()
                .HasKey(i => i.Id);

            modelBuilder.Entity<InviteCode>()
                .HasOne(i => i.Master)
                .WithMany()
                .HasForeignKey(i => i.MasterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Конфигурация модели Deck
            modelBuilder.Entity<Deck>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<Deck>()
                .HasMany(d => d.Cards)
                .WithOne(c => c.Deck)
                .HasForeignKey(c => c.DeckId);

            // Конфигурация модели Card
            modelBuilder.Entity<Card>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Card>()
                .HasMany(c => c.SessionCards)
                .WithOne(sc => sc.Card)
                .HasForeignKey(sc => sc.CardId);

            // Конфигурация модели SessionCard
            modelBuilder.Entity<SessionCard>()
                .HasKey(sc => new { sc.SessionId, sc.CardId });

            modelBuilder.Entity<SessionCard>()
                .HasOne(sc => sc.Session)
                .WithMany(s => s.ChoosenCards)
                .HasForeignKey(sc => sc.SessionId);

            modelBuilder.Entity<SessionCard>()
                .HasOne(sc => sc.Card)
                .WithMany(c => c.SessionCards)
                .HasForeignKey(sc => sc.CardId);

            // Конфигурация модели BotMessages
            modelBuilder.Entity<BotMessage>()
                .HasKey(bm => bm.Id);

            // Конфигурация модели SessionParameters
            modelBuilder.Entity<SessionParameters>()
                .HasKey(sp => sp.Id);

            // Конфигурация модели Feedback
            modelBuilder.Entity<Feedback>()
                .HasKey (f => f.Id);
        }
    }
}
