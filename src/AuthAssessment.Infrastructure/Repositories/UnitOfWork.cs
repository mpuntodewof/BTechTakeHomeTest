using AuthAssessment.Application.Interfaces;
using AuthAssessment.Infrastructure.Persistence;

namespace AuthAssessment.Infrastructure.Repositories;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);
}
