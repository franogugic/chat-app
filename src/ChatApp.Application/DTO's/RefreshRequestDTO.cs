namespace ChatApp.Application.DTO_s;

public class RefreshRequestDTO
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    
}