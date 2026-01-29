using ChatApp.Application.DTO_s;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _dbContext;
    
    public AuthRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext)); 
    }
    
    public async Task CreateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        
        cancellationToken.ThrowIfCancellationRequested();
        
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id must be a non-empty GUID.", nameof(id));
        
        cancellationToken.ThrowIfCancellationRequested();
        return await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetUserByMailAsync(string mail, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mail))
            throw new ArgumentException("Mail is required.", nameof(mail));

        var normalized = mail.Trim().ToLowerInvariant();
        
        cancellationToken.ThrowIfCancellationRequested();
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Mail.ToLower() == normalized, cancellationToken);
    }

    public async Task<List<User>> GetAllUsersBySearchAsync(Guid userId, string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking() 
            .Where(u => u.Name.ToLower().Contains(searchTerm.ToLower()))
            .Where(u => u.Id != userId)
            .Take(20)
            .ToListAsync(cancellationToken);
    }
}