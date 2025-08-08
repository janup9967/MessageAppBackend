using Microsoft.EntityFrameworkCore;
using MessageApp.Model;
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

        // Message → Sender
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Message → Receiver
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiveId)
            .OnDelete(DeleteBehavior.Restrict);

        // Message → Conversation
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId);

        // Conversation → Creator
        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.Creator)
            .WithMany(u => u.CreatedConversations)
            .HasForeignKey(c => c.CreatedByUser)
            .OnDelete(DeleteBehavior.Restrict);

        // Conversation → Receiver
        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.Receiver)
            .WithMany(u => u.ReceivedConversations)
            .HasForeignKey(c => c.ReceiveId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
