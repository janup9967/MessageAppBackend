using System.ComponentModel.DataAnnotations;

namespace MessageApp.Dtos
{

public class CreateMessageDto
{
    [Required]
    public string ReceiverIdentifier { get; set; } // Username or Email

    [Required]
    public string Content { get; set; }
}

}