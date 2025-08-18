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

    public class MarkReadDto
    {
        public List<int> MessageIds { get; set; }
    }

    public class UnreadMessageDto
    {
        public int Id { get; set; }                 // Message ID
        public int SenderId { get; set; }           // Sender user ID
        public string SenderUsername { get; set; }  // Sender username
        public int ReceiverId { get; set; }         // Receiver user ID
        public string ReceiverUsername { get; set; }// Receiver username
        public string Content { get; set; }         // Message content
        public DateTime Time { get; set; }          // Timestamp
        public bool IsRead { get; set; }            // Read status
        public int ConversationId { get; set; }     // Optional: Conversation ID
    }
}

