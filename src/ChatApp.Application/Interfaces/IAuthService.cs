using ChatApp.Application.DTO_s;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces;

public interface IAuthService
{
    Task<CreateUserResponseDTO> CreateUserAsync(CreateUserRequestDTO request, CancellationToken cancellationToken = default);
    Task<CreateUserResponseDTO?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<AuthResponseDTO> LoginAsync(LoginUserRequestDTO request, CancellationToken cancellationToken = default);
}