using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ChatApp.Application.DTO_s;
using ChatApp.Application.Exceptions;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace ChatApp.Infrastructure.Authentication;

public class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;
    private readonly IRefreshTokenRepository _rfRepository;
    private readonly ILogger<JwtProvider> _logger;
    
    public JwtProvider(IOptions<JwtOptions> options, IRefreshTokenRepository rfRepository, ILogger<JwtProvider> logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _rfRepository = rfRepository ?? throw new ArgumentNullException(nameof(rfRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Generate(User user)
    {
        var claims = new Claim[]
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Mail),
            new("name", user.Name)
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
                _options.Issuer,    
                _options.Audience,
                claims, 
                null, 
                DateTime.UtcNow.AddMinutes(_options.ExpireMinutes),
                credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public string GenerateRefreshToken() 
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
    
public async Task<AuthResponseDTO> RefreshToken(string accessToken, string refreshToken, CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(refreshToken))
    {
        _logger.LogError("Refresh token is missing.");
        throw new ArgumentException("Refresh token is required.");
    }

    var existingToken = await _rfRepository.GetByTokenAsync(refreshToken, cancellationToken);
    
    if (existingToken is null || existingToken.IsRevoked || existingToken.IsExpired) 
    {
        _logger.LogWarning("Invalid, revoked or expired refresh token attempt.");
        throw new RefreshTokenIsNotValidException();
    }

    if (!string.IsNullOrWhiteSpace(accessToken))
    {
        try 
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);
            var userIdFromToken = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                                  ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdFromToken != existingToken.UserId.ToString())
            {
                _logger.LogCritical("Security breach attempt: Access token UID mismatch with Refresh token!");
                throw new RefreshTokenIsNotValidException();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not parse expired access token: {Message}", ex.Message);
        }
    }

    existingToken.IsRevoked = true;
    
    var newAccessToken = Generate(existingToken.User);
    var newRefreshToken = GenerateRefreshToken();
    
    var newRefreshTokenEntity = Domain.Entities.RefreshToken.Create(
        newRefreshToken, 
        existingToken.UserId, 
        TimeSpan.FromDays(_options.RefreshTokenExpireDays));
    
    await _rfRepository.AddRFAsync(newRefreshTokenEntity, cancellationToken);
    await _rfRepository.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Successfully rotated tokens for user {UserId}.", existingToken.UserId);

    return new AuthResponseDTO(
        newAccessToken, 
        newRefreshTokenEntity.Token, 
        existingToken.UserId, 
        existingToken.User.Name, 
        existingToken.User.Mail);
}
    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, 
            ValidateIssuer = false,   
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token algorithm");
        }

        return principal;
    }
}