namespace ChatApp.Application.DTO_s;

public class CreateUserResponseDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Mail { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}