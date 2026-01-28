using ChatApp.Application.DTO_s;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces;

public interface IAuthRepository
{
    Task CreateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetUserByMailAsync(string mail, CancellationToken cancellationToken = default);
    Task<List<User>> GetAllUsersBySearchAsync(string searchTerm, CancellationToken cancellationToken = default);

}