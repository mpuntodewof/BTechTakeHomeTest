using AuthAssessment.Application.DTOs.Auth;
using AuthAssessment.Application.Interfaces;
using AuthAssessment.Application.UseCases;
using AuthAssessment.Domain.Entities;
using AuthAssessment.Domain.Enums;
using FluentAssertions;
using Moq;

namespace AuthAssessment.Tests.Unit.UseCases;

public class AuthUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly AuthUseCase _sut;

    public AuthUseCaseTests()
    {
        _sut = new AuthUseCase(
            _userRepository.Object,
            _refreshTokenRepository.Object,
            _tokenService.Object,
            _passwordHasher.Object,
            _unitOfWork.Object);
    }

    private static User CreateUser(string email = "test@example.com") => new()
    {
        Id = Guid.NewGuid(),
        Email = email,
        PasswordHash = "hashed-password",
        FullName = "Test User",
        Balance = 10_000m,
        Role = UserRole.User,
        CreatedAt = DateTime.UtcNow
    };

    private RegisterRequest CreateRegisterRequest(string email = "test@example.com", string password = "Password123", string fullName = "Test User")
        => new() { Email = email, Password = password, ConfirmPassword = password, FullName = fullName };

    #region Login Tests

    [Fact]
    public async Task Login_Should_ReturnTokens_OnValidCredentials()
    {
        // Arrange
        var user = CreateUser();
        var request = new LoginRequest { Email = "test@example.com", Password = "Password123" };

        _userRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("Password123", "hashed-password")).Returns(true);
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("raw-refresh-token");
        _tokenService.Setup(t => t.HashToken("raw-refresh-token")).Returns("hashed-refresh-token");
        _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("access-token");

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("raw-refresh-token");
        result.User.Id.Should().Be(user.Id);
        result.User.Email.Should().Be(user.Email);

        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_Should_Fail_WhenEmailNotFound()
    {
        // Arrange
        var request = new LoginRequest { Email = "notfound@example.com", Password = "Password123" };
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = () => _sut.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task Login_Should_Fail_WhenPasswordIncorrect()
    {
        // Arrange
        var user = CreateUser();
        var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword" };

        _userRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("WrongPassword", "hashed-password")).Returns(false);

        // Act
        Func<Task> act = () => _sut.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid email or password*");
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task Register_Should_CreateUser_WithHashedPassword_AndSeedBalance()
    {
        // Arrange
        var request = CreateRegisterRequest();
        _userRepository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash(request.Password)).Returns("hashed-password");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("raw-refresh-token");
        _tokenService.Setup(t => t.HashToken("raw-refresh-token")).Returns("hashed-refresh-token");
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");

        User? capturedUser = null;
        _userRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u);

        // Act
        await _sut.RegisterAsync(request);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().Be("hashed-password");
        capturedUser.Balance.Should().Be(10_000m);
        capturedUser.Role.Should().Be(UserRole.User);
        capturedUser.Email.Should().Be("test@example.com");
        capturedUser.FullName.Should().Be("Test User");

        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_Should_Fail_WhenEmailAlreadyExists()
    {
        // Arrange
        var request = CreateRegisterRequest();
        _userRepository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = () => _sut.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already registered*");
    }

    [Fact]
    public async Task Register_Should_ReturnTokens_OnSuccessfulRegistration()
    {
        // Arrange
        var request = CreateRegisterRequest();
        _userRepository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed-password");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("raw-refresh-token");
        _tokenService.Setup(t => t.HashToken("raw-refresh-token")).Returns("hashed-refresh-token");
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("raw-refresh-token");
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be("test@example.com");
        result.User.FullName.Should().Be("Test User");
        result.User.Balance.Should().Be(10_000m);
        result.User.Role.Should().Be("User");
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshToken_Should_RotateTokens_OnValidRefreshToken()
    {
        // Arrange
        var user = CreateUser();
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "hashed-old-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        var request = new RefreshTokenRequest { RefreshToken = "old-raw-token" };

        _tokenService.Setup(t => t.HashToken("old-raw-token")).Returns("hashed-old-token");
        _refreshTokenRepository.Setup(r => r.GetByTokenAsync("hashed-old-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _userRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("new-raw-token");
        _tokenService.Setup(t => t.HashToken("new-raw-token")).Returns("hashed-new-token");
        _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("new-access-token");

        // Act
        var result = await _sut.RefreshTokenAsync(request);

        // Assert
        storedToken.IsRevoked.Should().BeTrue();
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-raw-token");
        result.User.Id.Should().Be(user.Id);

        _refreshTokenRepository.Verify(r => r.AddAsync(It.Is<RefreshToken>(rt => rt.Token == "hashed-new-token"), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_Should_Fail_WhenTokenExpired()
    {
        // Arrange
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "hashed-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };
        var request = new RefreshTokenRequest { RefreshToken = "raw-token" };

        _tokenService.Setup(t => t.HashToken("raw-token")).Returns("hashed-token");
        _refreshTokenRepository.Setup(r => r.GetByTokenAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        Func<Task> act = () => _sut.RefreshTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*expired or revoked*");
    }

    [Fact]
    public async Task RefreshToken_Should_Fail_WhenTokenRevoked()
    {
        // Arrange
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "hashed-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = true,
            CreatedAt = DateTime.UtcNow
        };
        var request = new RefreshTokenRequest { RefreshToken = "raw-token" };

        _tokenService.Setup(t => t.HashToken("raw-token")).Returns("hashed-token");
        _refreshTokenRepository.Setup(r => r.GetByTokenAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        Func<Task> act = () => _sut.RefreshTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*expired or revoked*");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task Register_Should_NormalizeEmail_ToLowerCase()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "USER@EXAMPLE.COM",
            Password = "Password123",
            ConfirmPassword = "Password123",
            FullName = "Test User"
        };
        _userRepository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("rt");
        _tokenService.Setup(t => t.HashToken("rt")).Returns("hrt");
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("at");

        User? capturedUser = null;
        _userRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u);

        // Act
        await _sut.RegisterAsync(request);

        // Assert
        capturedUser!.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task Login_Should_NormalizeEmail_ToLowerCase()
    {
        // Arrange
        var user = CreateUser();
        var request = new LoginRequest { Email = "TEST@EXAMPLE.COM", Password = "Password123" };

        _userRepository.Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("Password123", "hashed-password")).Returns(true);
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("rt");
        _tokenService.Setup(t => t.HashToken("rt")).Returns("hrt");
        _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("at");

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        _userRepository.Verify(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_Should_Fail_WhenTokenNotFound()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "nonexistent-token" };
        _tokenService.Setup(t => t.HashToken("nonexistent-token")).Returns("hashed-nonexistent");
        _refreshTokenRepository.Setup(r => r.GetByTokenAsync("hashed-nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        Func<Task> act = () => _sut.RefreshTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid refresh token*");
    }

    [Fact]
    public async Task RefreshToken_Should_Fail_WhenUserNotFound()
    {
        // Arrange
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "hashed-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        var request = new RefreshTokenRequest { RefreshToken = "raw-token" };

        _tokenService.Setup(t => t.HashToken("raw-token")).Returns("hashed-token");
        _refreshTokenRepository.Setup(r => r.GetByTokenAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _userRepository.Setup(r => r.GetByIdAsync(storedToken.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = () => _sut.RefreshTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*User not found*");
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_Should_RevokeAllTokens_ForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        await _sut.LogoutAsync(userId);

        // Assert
        _refreshTokenRepository.Verify(r => r.RevokeAllByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
