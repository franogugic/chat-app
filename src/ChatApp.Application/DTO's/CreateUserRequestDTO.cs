using System.ComponentModel.DataAnnotations;

namespace ChatApp.Application.DTO_s;

public class CreateUserRequestDTO
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = null!;
    [Required]
    [EmailAddress]
    public string Mail { get; set; } = null!;
    [Required]
    [MinLength(4)]
    public string Password { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}