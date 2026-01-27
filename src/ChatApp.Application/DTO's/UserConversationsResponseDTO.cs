using ChatApp.Domain.Entities;

namespace ChatApp.Application.DTO_s;

public class UserConversationsResponseDTO
{
    public string? Title { get; set; } = String.Empty;
    public MessageDTO LastMessage { get; set; }
}

public class MessageDTO
{
    public DateTime SentAt { get; set; }
    public string Content { get; set; }
    public bool IsRead { get; set; } 
    public DateTime? ReadAt { get; set; } 
}
