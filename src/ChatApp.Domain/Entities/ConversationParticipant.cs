namespace ChatApp.Domain.Entities;

public class ConversationParticipant
{
    public DateTime JoinedAt { get; private set; }
    public bool IsAdmin { get; private set; }
    
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    
    public Guid ConversationId { get; private set; }
    public Conversation Conversation { get; private set; } = null!;
    
    public ConversationParticipant() { }

    public static ConversationParticipant Create(Guid userId, Guid conversationId, bool isAdmin = false)
    {
        return new ConversationParticipant
        {
            UserId = userId,
            ConversationId = conversationId,
            JoinedAt = DateTime.UtcNow,
            IsAdmin = isAdmin
        };
    }
    
    public void PromoteToAdmin()
    {
        IsAdmin = true;
    }
    
    public void DemoteFromAdmin()
    {
        IsAdmin = false;
    }
}