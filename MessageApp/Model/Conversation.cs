using System.Text.Json.Serialization;
using MessageApp.Model;

namespace MessageApp.Model
{
    public class Conversation
    {
        public int Id { get; set; }
        public int CreatedByUser { get; set; }
        public int ReceiveId { get; set; }
        public List<Message>? Messages { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties

        [JsonIgnore]
        public User Creator { get; set; }

        [JsonIgnore]
        public User Receiver { get; set; }
    }

}