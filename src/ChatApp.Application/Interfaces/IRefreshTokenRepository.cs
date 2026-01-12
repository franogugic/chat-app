using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddRFAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    
    Task SaveChangesAsync(CancellationToken ct);
}