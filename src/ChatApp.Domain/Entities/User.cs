namespace ChatApp.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Mail { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string PhoneNumber { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();
    
    private User() { }

    public static User Create(String name, String mail, String passwordHash, String phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        
        if (string.IsNullOrWhiteSpace(mail))
            throw new ArgumentException("Mail is required", nameof(mail));
        
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password is required", nameof(passwordHash));
        
        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Mail = mail, 
            PasswordHash = passwordHash,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow
        };
    }
}