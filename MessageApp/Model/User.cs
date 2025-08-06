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

        public ICollection<ConversationUser> ConversationUsers { get; set; }
        public ICollection<Message> Messages { get; set; }
    }

}