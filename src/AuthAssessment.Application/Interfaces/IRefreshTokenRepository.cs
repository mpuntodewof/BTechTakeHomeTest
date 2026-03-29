using AuthAssessment.Domain.Entities;

namespace AuthAssessment.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task<RefreshToken?> GetByTokenAsync(string tokenHash, CancellationToken ct = default);
    Task RevokeAllByUserIdAsync(Guid userId, CancellationToken ct = default);
}
