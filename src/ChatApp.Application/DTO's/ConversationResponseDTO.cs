namespace ChatApp.Application.DTO_s;

public class ConversationDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public bool IsGroup { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ParticipantDto> Participants { get; set; } = new();
}

public class ParticipantDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}