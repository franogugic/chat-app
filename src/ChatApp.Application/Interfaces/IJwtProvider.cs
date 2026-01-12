using ChatApp.Application.DTO_s;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces;

public interface IJwtProvider
{
    string Generate(User user);
    string GenerateRefreshToken();
    
    Task<AuthResponseDTO> RefreshToken(string token, CancellationToken cancellationToken = default);
}