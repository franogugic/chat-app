using ChatApp.Application.Interfaces;

namespace ChatApp.Infrastructure.Security;

public sealed class PasswordHash : IPasswordHash
{
    public async Task<string> HashPasswordAsync(string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password must be provided.", nameof(password));

        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return BCrypt.Net.BCrypt.HashPassword(password);
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> VerifyPasswordAsync (string hashedPassword, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentException("Hashed password must be provided.", nameof(hashedPassword));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password must be provided.", nameof(password));

        return await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }, cancellationToken).ConfigureAwait(false);
    }
}