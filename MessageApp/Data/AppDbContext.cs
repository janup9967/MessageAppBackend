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
    public DbSet<ConversationUser> ConversationUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Example: Composite key for ConversationUser
        modelBuilder.Entity<ConversationUser>()
            .HasKey(cu => new { cu.ConversationId, cu.UserId });

        // Relationships
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId);

        modelBuilder.Entity<ConversationUser>()
            .HasOne(cu => cu.Conversation)
            .WithMany(c => c.ConversationUsers)
            .HasForeignKey(cu => cu.ConversationId);

        modelBuilder.Entity<ConversationUser>()
            .HasOne(cu => cu.User)
            .WithMany(u => u.ConversationUsers)
            .HasForeignKey(cu => cu.UserId);
    }
}