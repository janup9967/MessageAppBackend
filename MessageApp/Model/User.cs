using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


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
    
    [JsonIgnore]
    public ICollection<Message> SentMessages { get; set; }

    [JsonIgnore]
    public ICollection<Message> ReceivedMessages { get; set; }

    [JsonIgnore]
    public ICollection<Conversation> CreatedConversations { get; set; }

    [JsonIgnore]
    public ICollection<Conversation> ReceivedConversations { get; set; }
}

}