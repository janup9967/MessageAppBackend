using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace MessageApp.Model
{
public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<Message> SentMessages { get; set; }
    public ICollection<Message> ReceivedMessages { get; set; }
    public ICollection<Conversation> CreatedConversations { get; set; }
    public ICollection<Conversation> ReceivedConversations { get; set; }
}

}