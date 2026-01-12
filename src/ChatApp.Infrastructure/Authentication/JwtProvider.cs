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
        _logger.LogInformation("Generating Access Token for user: {Email}", user.Mail);

        var claims = new Claim[]
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Mail),
            new("name", user.Name)
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        // Ovdje koristimo npr. 15-60 minuta za Access Token (podesi u JwtOptions ako želiš)
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
    
    public async Task<AuthResponseDTO> RefreshToken(string token, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Refresh attempt with empty token.");
            throw new ArgumentException("Token is required", nameof(token));
        }

        _logger.LogInformation("Attempting to refresh token...");

        var existingToken = await _rfRepository.GetByTokenAsync(token, cancellationToken);
        
        if(existingToken is null) 
        {
            _logger.LogWarning("Refresh failed: Token does not exist in database.");
            throw new RefreshTokenIsNotValidException();
        }

        if (existingToken.IsRevoked)
        {
            _logger.LogCritical("SECURITY ALERT: Attempted reuse of REVOKED token by user {UserId}!", existingToken.UserId);
            throw new RefreshTokenIsNotValidException();
        }

        if (existingToken.IsExpired)
        {
            _logger.LogWarning("Refresh failed: Token for user {UserId} has expired on {ExpiryDate}.", existingToken.UserId, existingToken.ExpiryDate);
            throw new RefreshTokenIsNotValidException();
        }

        existingToken.IsRevoked = true;
        
        var newAccessToken = Generate(existingToken.User);
        var newRefreshToken = GenerateRefreshToken();
        
        TimeSpan validityPeriod = TimeSpan.FromDays(_options.RefreshTokenExpireDays);
        _logger.LogWarning("VALIDATY PERIOD: {ValidityPeriod}", validityPeriod);
        
        var newRefreshTokenEntity = Domain.Entities.RefreshToken.Create(
            newRefreshToken, 
            existingToken.UserId, 
            validityPeriod);
        
        await _rfRepository.AddRFAsync(newRefreshTokenEntity, cancellationToken);
        
        await _rfRepository.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Successfully rotated tokens for user {UserId}. New RF expiration: {Expiry}", 
            existingToken.UserId, newRefreshTokenEntity.ExpiryDate);

        return new AuthResponseDTO(newAccessToken, newRefreshTokenEntity.Token);
    }
}