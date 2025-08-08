using System.Text.Json.Serialization;
using MessageApp.Model;

namespace MessageApp.Model
{


  public class Message
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiveId { get; set; }
    public int ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public bool IsRead { get; set; }

    // Navigation Properties
    [JsonIgnore]
    public User Sender { get; set; }
    
    [JsonIgnore]
    public User Receiver { get; set; }

    [JsonIgnore]
    public Conversation Conversation { get; set; }
}


}