using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _dbContext;
    
    public RefreshTokenRepository(AppDbContext dbContext)
    {
        _dbContext =  dbContext;    
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        return await _dbContext.RefreshTokens
            .Include(u => u.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken )
            .ConfigureAwait(false);
    }
    
    public async Task AddRFAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);
        
        cancellationToken.ThrowIfCancellationRequested();
        
        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
