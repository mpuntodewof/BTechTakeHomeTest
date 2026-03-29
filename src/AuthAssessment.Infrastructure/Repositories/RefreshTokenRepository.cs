using AuthAssessment.Application.Interfaces;
using AuthAssessment.Domain.Entities;
using AuthAssessment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthAssessment.Infrastructure.Repositories;

public sealed class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
        => await context.RefreshTokens.AddAsync(token, ct);

    public async Task<RefreshToken?> GetByTokenAsync(string tokenHash, CancellationToken ct = default)
        => await context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == tokenHash, ct);

    public async Task RevokeAllByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        await context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsRevoked, true), ct);
    }
}
