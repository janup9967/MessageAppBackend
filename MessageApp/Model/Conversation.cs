using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MessageApp.Model
{
public class Conversation
{
    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ConversationUser> ConversationUsers { get; set; }
    public ICollection<Message> Messages { get; set; }
}
    
}
