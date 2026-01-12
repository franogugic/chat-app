namespace ChatApp.Application.Interfaces;

public interface IPasswordHash
{
    Task <string> HashPasswordAsync(string password, CancellationToken cancellationToken = default);
    Task <bool> VerifyPasswordAsync(string hashedPassword, string password,CancellationToken cancellationToken = default);
}