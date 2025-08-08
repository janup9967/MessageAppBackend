using Microsoft.EntityFrameworkCore;
using MessageApp.Model;

namespace MessageApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🧍 User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Username)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(u => u.Email)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.PasswordHash)
                    .IsRequired();

                entity.HasIndex(u => u.Username)
                    .IsUnique();

                entity.HasIndex(u => u.Email)
                    .IsUnique();
            });

            // 💬 Message Configuration
            modelBuilder.Entity<Message>(entity =>
            {
                entity.Property(m => m.Content)
                    .IsRequired();

                entity.Property(m => m.Time)
                    .IsRequired();

                entity.Property(m => m.IsRead)
                    .IsRequired();

                entity.HasOne(m => m.Sender)
                    .WithMany(u => u.SentMessages)
                    .HasForeignKey(m => m.SenderId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Receiver)
                    .WithMany(u => u.ReceivedMessages)
                    .HasForeignKey(m => m.ReceiveId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Conversation)
                    .WithMany(c => c.Messages)
                    .HasForeignKey(m => m.ConversationId)
                    .IsRequired();

                entity.HasIndex(m => m.ConversationId);
            });

            // 🗨️ Conversation Configuration
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.Property(c => c.CreatedAt)
                    .IsRequired();

                entity.HasOne(c => c.Creator)
                    .WithMany(u => u.CreatedConversations)
                    .HasForeignKey(c => c.CreatedByUser)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Receiver)
                    .WithMany(u => u.ReceivedConversations)
                    .HasForeignKey(c => c.ReceiveId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}