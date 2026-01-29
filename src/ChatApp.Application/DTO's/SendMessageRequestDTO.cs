using System.ComponentModel.DataAnnotations;
using ChatApp.Domain.Enums;

namespace ChatApp.Application.DTO_s;

public class SendMessageRequestDTO
{
    public Guid ConversationId { get; set; }
    
    public Guid RecipientId { get; set; }
    
    [Required]
    public string Content { get; set; } = null!;
    
    public MessageType MessageType { get; set; }
}