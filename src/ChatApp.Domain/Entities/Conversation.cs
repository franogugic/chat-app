namespace ChatApp.Domain.Entities;

public class Conversation
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public bool IsGroup { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public Guid? LastMessageId { get; private set; }
    public Message? LastMessage { get; private set; }
    
    public ICollection<ConversationParticipant> Participants { get; private set; } = new List<ConversationParticipant>();
    public ICollection<Message> Messages { get; private set; } = new List<Message>();
    
    private Conversation() {}

    public static Conversation Create(string title, bool isGroup)
    {
        return new Conversation
        {
            Id = Guid.NewGuid(),
            LastMessageId = null,
            Title = isGroup ? title! : string.Empty,
            IsGroup = isGroup,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public void UpdateLastMessage(Guid messageId)
    {
        LastMessageId = messageId;
    }
    
    public void SetTitle(string title)
    {
        if (IsGroup)
        {
            Title = title;
        }
    }

    public void AddParticipant(Guid userId, bool isAdmin = false)
    {
        if(Participants.Any(p => p.UserId == userId))
            return; 
        
        var participant = ConversationParticipant.Create(userId, this.Id, isAdmin);
        
        Participants.Add(participant);
    }
}