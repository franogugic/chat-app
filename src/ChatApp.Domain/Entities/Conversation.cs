namespace ChatApp.Domain.Entities;

public class Conversation
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public bool IsGroup { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public Guid? LastMessageId { get; private set; }
}