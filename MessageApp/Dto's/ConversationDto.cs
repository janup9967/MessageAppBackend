using System.ComponentModel.DataAnnotations;

namespace MessageApp.Dtos
{
    public class CreateConversationDto
    {
        [Required]
        public string ReceiverIdentifier { get; set; } // Username or Email
    }

    public class ConversationDto
{
    public int Id { get; set; }
    public string ReceiverUsername { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? LastMessageContent { get; set; }
    public DateTime? LastMessageTime { get; set; }
    public bool? LastMessageIsRead { get; set; }
}


}