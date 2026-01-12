using AutoMapper;
using ChatApp.Application.DTO_s;
using ChatApp.Application.Exceptions;
using ChatApp.Application.Interfaces;
using ChatApp.Application.Services;
using ChatApp.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ChatApp.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IPasswordHash> _passwordHashMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IRefreshTokenRepository> _rfRepositoryMock;
    private readonly IOptions<JwtOptions> _options;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _passwordHashMock = new Mock<IPasswordHash>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _rfRepositoryMock = new Mock<IRefreshTokenRepository>();
        
        var jwtOptions = new JwtOptions { RefreshTokenExpireDays = 7 };
        _options = Options.Create(jwtOptions);

        _sut = new AuthService(
            _authRepositoryMock.Object, 
            _passwordHashMock.Object, 
            _mapperMock.Object, 
            _loggerMock.Object, 
            _jwtProviderMock.Object, 
            _options, 
            _rfRepositoryMock.Object);
    }

    #region CreateUserAsync

    [Fact]
    public async Task CreateUserAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // ACT
        var act = () => _sut.CreateUserAsync(null!);

        // ASSERT
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldThrowUserAlreadyExistsException_WhenEmailAlreadyInUse()
    {
        // ARRANGE
        var request = new CreateUserRequestDTO { Name = "Frano", Mail = "test@test.com", Password = "password123" };
        _authRepositoryMock.Setup(x => x.GetUserByMailAsync(request.Mail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(User.Create("Existing", request.Mail, "hash", "123"));

        // ACT
        var act = () => _sut.CreateUserAsync(request);

        // ASSERT
        await act.Should().ThrowAsync<UserAlreadyExistsException>();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldSucceed_WhenDataIsValid()
    {
        // ARRANGE
        var request = new CreateUserRequestDTO { Name = "Frano", Mail = "new@test.com", Password = "password123" };
        _authRepositoryMock.Setup(x => x.GetUserByMailAsync(request.Mail, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _passwordHashMock.Setup(x => x.HashPasswordAsync(request.Password, It.IsAny<CancellationToken>())).ReturnsAsync("hashed_pass");
        
        // ACT
        await _sut.CreateUserAsync(request);

        // ASSERT
        _authRepositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region LoginAsync

    [Fact]
    public async Task LoginAsync_ShouldThrowArgumentException_WhenMailIsEmpty()
    {
        // ARRANGE
        var request = new LoginUserRequestDTO { Mail = "", Password = "123" };

        // ACT
        var act = () => _sut.LoginAsync(request);

        // ASSERT
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Mail*");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowArgumentException_WhenPasswordIsEmpty()
    {
        // ARRANGE
        var request = new LoginUserRequestDTO { Mail = "test@test.com", Password = "" };

        // ACT
        var act = () => _sut.LoginAsync(request);

        // ASSERT
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Password*");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUserNotFoundException_WhenUserDoesNotExist()
    {
        // ARRANGE
        var request = new LoginUserRequestDTO { Mail = "nonexistent@test.com", Password = "123" };
        _authRepositoryMock.Setup(x => x.GetUserByMailAsync(request.Mail, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // ACT
        var act = () => _sut.LoginAsync(request);

        // ASSERT
        await act.Should().ThrowAsync<UserNotFoundByMailException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowIncorrectPasswordException_WhenPasswordIsWrong()
    {
        // ARRANGE
        var request = new LoginUserRequestDTO { Mail = "test@test.com", Password = "wrong_password" };
        var user = User.Create("Frano", request.Mail, "correct_hash", "123");
        _authRepositoryMock.Setup(x => x.GetUserByMailAsync(request.Mail, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHashMock.Setup(x => x.VerifyPasswordAsync(user.PasswordHash, request.Password, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // ACT
        var act = () => _sut.LoginAsync(request);

        // ASSERT
        await act.Should().ThrowAsync<IncorrectPasswordException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreCorrect()
    {
        // ARRANGE
        var request = new LoginUserRequestDTO { Mail = "test@test.com", Password = "correct" };
        var user = User.Create("Frano", request.Mail, "hash", "123");
        _authRepositoryMock.Setup(x => x.GetUserByMailAsync(request.Mail, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHashMock.Setup(x => x.VerifyPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _jwtProviderMock.Setup(x => x.Generate(user)).Returns("access_token");
        _jwtProviderMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");

        // ACT
        var result = await _sut.LoginAsync(request);

        // ASSERT
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task LoginAsync_ShouldSaveRefreshToken_WhenLoginSucceeds()
    {
        // ARRANGE
        var request = new LoginUserRequestDTO { Mail = "test@test.com", Password = "correct" };
        var user = User.Create("Frano", request.Mail, "hash", "123");
        _authRepositoryMock.Setup(x => x.GetUserByMailAsync(request.Mail, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHashMock.Setup(x => x.VerifyPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // ACT
        await _sut.LoginAsync(request);

        // ASSERT
        _rfRepositoryMock.Verify(x => x.AddRFAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _rfRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetUserByIdAsync

    [Fact]
    public async Task GetUserByIdAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
    {
        // ACT
        var act = () => _sut.GetUserByIdAsync(Guid.Empty);

        // ASSERT
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // ARRANGE
        var userId = Guid.NewGuid();
        _authRepositoryMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // ACT
        var result = await _sut.GetUserByIdAsync(userId);

        // ASSERT
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // ARRANGE
        var userId = Guid.NewGuid();
        var user = User.Create("Frano", "test@test.com", "hash", "123");
        _authRepositoryMock.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mapperMock.Setup(x => x.Map<CreateUserResponseDTO>(user)).Returns(new CreateUserResponseDTO { Mail = user.Mail });

        // ACT
        var result = await _sut.GetUserByIdAsync(userId);

        // ASSERT
        result.Should().NotBeNull();
        result!.Mail.Should().Be(user.Mail);
    }

    #endregion

    #region Resilience & Cancellation

    [Fact]
    public async Task CreateUserAsync_ShouldPropagateCancellation_WhenTokenIsCancelled()
    {
        // ARRANGE
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var request = new CreateUserRequestDTO 
        { 
            Name = "Frano", 
            Mail = "test@test.com", 
            Password = "Password123!", 
            PhoneNumber = "123456789" 
        };

        _authRepositoryMock
            .Setup(x => x.GetUserByMailAsync(request.Mail, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        // ACT
        var act = () => _sut.CreateUserAsync(request, cts.Token);

        // ASSERT
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
    
    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenDatabaseFails()
    {
        // ARRANGE
        var request = new LoginUserRequestDTO { Mail = "test@test.com", Password = "123" };
        _authRepositoryMock.Setup(x => x.GetUserByMailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database offline"));

        // ACT
        var act = () => _sut.LoginAsync(request);

        // ASSERT
        await act.Should().ThrowAsync<Exception>().WithMessage("Database offline");
    }

    #endregion
}


