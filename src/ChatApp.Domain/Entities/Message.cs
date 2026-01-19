using ChatApp.Domain.Enums;

namespace ChatApp.Domain.Entities;

public class Message
{
    public Guid Id { get; private set; }
    public string Content { get; private set; } = string.Empty;
    
    public MessageType Type { get; private set; } = MessageType.text;
    public DateTime SentAt { get; private set; }
    public bool IsRead { get; private set; } 
    public DateTime? ReadAt { get; private set; } 
    
    public Guid SenderId { get; private set; }
    public User User { get; private set; } = null!;
    
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = null!;
    
    private Message() { }

    public static Message Create(Guid senderId, Guid conversationId, string content, MessageType type = MessageType.text)
    {
        return new Message
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            ConversationId = conversationId,
            Content = content,
            Type = type,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            ReadAt = null
        };
    }
    
    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }
}