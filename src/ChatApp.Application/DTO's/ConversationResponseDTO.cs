namespace ChatApp.Application.DTO_s;

public class ConversationResponseDTO
{
    public Guid Id { get; set; }
    public bool IsGroup { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public Guid PartnerId { get; set; }
}