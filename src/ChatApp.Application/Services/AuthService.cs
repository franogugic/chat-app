using AutoMapper;
using ChatApp.Application.DTO_s;
using ChatApp.Application.Exceptions;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHash _passwordHash;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IJwtProvider _jwtProvider;
    private readonly JwtOptions _options;
    private readonly IRefreshTokenRepository _rfRepository;

    public AuthService(IAuthRepository authRepository, IPasswordHash passwordHash, IMapper mapper, ILogger<AuthService> logger, IJwtProvider jwtProvider,IOptions<JwtOptions> options, IRefreshTokenRepository rfRepository)
    {
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _passwordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _rfRepository = rfRepository ?? throw new ArgumentNullException(nameof(rfRepository));

    }

    public async Task<CreateUserResponseDTO> CreateUserAsync(CreateUserRequestDTO request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogDebug("CreateUserAsync called for mail {Mail}", request.Mail);

        var existingUser = await _authRepository
            .GetUserByMailAsync(request.Mail, cancellationToken);
        if (existingUser is not null)
        {
            _logger.LogInformation("Attempt to create existing user {Mail}", request.Mail);
            throw new UserAlreadyExistsException(request.Mail);
        }

        var passwordHash = await _passwordHash
            .HashPasswordAsync(request.Password, cancellationToken);

        var user = User.Create(request.Name, request.Mail, passwordHash, request.PhoneNumber ?? "N/A");

        await _authRepository
            .CreateUserAsync(user, cancellationToken);
        _logger.LogInformation("User created {UserId} for mail {Mail}", user.Id, request.Mail);
        
        return _mapper.Map<CreateUserResponseDTO>(user);
    }

    public async Task<CreateUserResponseDTO?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id must be a non-empty GUID.", nameof(id));

        var user = await _authRepository
            .GetUserByIdAsync(id, cancellationToken);
        return user is null ? null : _mapper.Map<CreateUserResponseDTO>(user);
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginUserRequestDTO request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Mail))
            throw new ArgumentException("Mail must not be empty.", nameof(request.Mail));
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password must not be empty.", nameof(request.Password));
        
        _logger.LogDebug("LoginAsync called for mail {Mail}", request.Mail);

        var user = await _authRepository
            .GetUserByMailAsync(request.Mail, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Attempt to login non-existing user {Mail}", request.Mail);
            throw new UserNotFoundByMailException(request.Mail);
        }

        var isPasswordValid = await _passwordHash
            .VerifyPasswordAsync(user.PasswordHash, request.Password, cancellationToken);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Invalid password attempt for user {UserId}", user.Id);
            throw new IncorrectPasswordException();
        }

        var token = _jwtProvider.Generate(user);
        var refreshTokenValue = _jwtProvider.GenerateRefreshToken();
        
        TimeSpan validityPeriod = TimeSpan.FromDays(_options.RefreshTokenExpireDays);
        var refreshTokenEntity = RefreshToken.Create(refreshTokenValue, user.Id, validityPeriod);
        
        await _rfRepository.AddRFAsync(refreshTokenEntity, cancellationToken);
        await _rfRepository.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("User {UserId} logged in {refreshTokenEntity}", user.Id, refreshTokenEntity.Token);
        
        return new AuthResponseDTO(token, refreshTokenValue, user.Id, user.Name, user.Mail);
    }
    
    public async Task<AuthResponseDTO?> RefreshTokenAsync(RefreshRequestDTO request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken) || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return null;
        }
        
        return await _jwtProvider.RefreshToken(request.AccessToken, request.RefreshToken, cancellationToken);
            
    }
}
