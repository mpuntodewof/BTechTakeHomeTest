using AuthAssessment.Application.DTOs.Auth;
using AuthAssessment.Application.Interfaces;
using AuthAssessment.Domain.Entities;
using AuthAssessment.Domain.Enums;

namespace AuthAssessment.Application.UseCases;

public sealed class AuthUseCase(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenService tokenService,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await userRepository.ExistsByEmailAsync(request.Email, ct))
            throw new InvalidOperationException("Email is already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            Balance = 10_000m,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, ct);

        var (accessToken, rawRefreshToken) = await GenerateTokensAsync(user, ct);

        return new AuthResponse(
            accessToken,
            rawRefreshToken,
            new UserInfo(user.Id, user.Email, user.FullName, user.Balance, user.Role.ToString())
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), ct)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var (accessToken, rawRefreshToken) = await GenerateTokensAsync(user, ct);

        return new AuthResponse(
            accessToken,
            rawRefreshToken,
            new UserInfo(user.Id, user.Email, user.FullName, user.Balance, user.Role.ToString())
        );
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var tokenHash = tokenService.HashToken(request.RefreshToken);
        var storedToken = await refreshTokenRepository.GetByTokenAsync(tokenHash, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (storedToken.IsRevoked || storedToken.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");

        storedToken.IsRevoked = true;

        var user = await userRepository.GetByIdAsync(storedToken.UserId, ct)
            ?? throw new UnauthorizedAccessException("User not found.");

        var (accessToken, rawRefreshToken) = await GenerateTokensAsync(user, ct);

        return new AuthResponse(
            accessToken,
            rawRefreshToken,
            new UserInfo(user.Id, user.Email, user.FullName, user.Balance, user.Role.ToString())
        );
    }

    public async Task LogoutAsync(Guid userId, CancellationToken ct = default)
    {
        await refreshTokenRepository.RevokeAllByUserIdAsync(userId, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<(string AccessToken, string RawRefreshToken)> GenerateTokensAsync(
        User user, CancellationToken ct)
    {
        var rawRefreshToken = tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokenService.HashToken(rawRefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await refreshTokenRepository.AddAsync(refreshToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var accessToken = tokenService.GenerateAccessToken(user);
        return (accessToken, rawRefreshToken);
    }
}
