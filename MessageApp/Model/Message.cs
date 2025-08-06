using System;
using System.ComponentModel.DataAnnotations;

namespace MessageApp.Model
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public int ConversationId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        public User Sender { get; set; }
        public Conversation Conversation { get; set; }
    }
}