using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MessageApp.Dtos
{

    public class CreateMessageDto
    {
        [Required]
        public string ReceiverIdentifier { get; set; } // Username or Email

        [Required]
        public string Content { get; set; }
    }

    public class MessageReturnDto
    {

        
// [JsonPropertyName("messageId")]

        public int Id { get; set; }
        public int SenderId { get; set; }
         public int ReceiveId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string ReceiverUsername { get; set; } = string.Empty;
        public int ConversationId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public bool IsRead { get; set; }
    }

}