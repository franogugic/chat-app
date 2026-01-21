namespace ChatApp.Application.DTO_s;

public record AuthResponseDTO(string AccessToken, string RefreshToken, Guid UserId, String name, String mail);