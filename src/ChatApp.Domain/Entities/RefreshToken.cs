namespace ChatApp.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => !IsRevoked && !IsExpired;
    
    private RefreshToken() { }
    
    public static RefreshToken Create (string token, Guid userId, TimeSpan validityPeriod)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            ExpiryDate = DateTime.UtcNow.Add(validityPeriod),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };
    }
}