using System.ComponentModel.DataAnnotations;

namespace ChatApp.Application.DTO_s;

public class RefreshRequestDTO
{
    public string AccessToken { get; set; } = null!;
    [Required]
    public string RefreshToken { get; set; } = null!;
}