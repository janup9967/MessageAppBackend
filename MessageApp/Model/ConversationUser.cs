using System.ComponentModel.DataAnnotations;

namespace MessageApp.Model
{
    public class ConversationUser
    {
        public int Id { get; set; }

        [Required]
        public int ConversationId { get; set; }

        [Required]
        public int UserId { get; set; }

        public Conversation Conversation { get; set; }
        public User User { get; set; }
    }

}