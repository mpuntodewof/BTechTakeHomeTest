using AuthAssessment.Application.Interfaces;
using AuthAssessment.Domain.Entities;
using AuthAssessment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthAssessment.Infrastructure.Repositories;

public sealed class TransactionRepository(AppDbContext context) : ITransactionRepository
{
    public async Task AddAsync(Transaction transaction, CancellationToken ct = default)
        => await context.Transactions.AddAsync(transaction, ct);

    public async Task<bool> ExistsByIdempotencyKeyAsync(Guid idempotencyKey, CancellationToken ct = default)
        => await context.Transactions.AnyAsync(t => t.IdempotencyKey == idempotencyKey, ct);

    public async Task<(List<Transaction> Items, int TotalCount)> GetByUserIdAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.Transactions
            .AsNoTracking()
            .Include(t => t.Sender)
            .Include(t => t.Recipient)
            .Where(t => t.SenderId == userId || t.RecipientId == userId)
            .OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<(List<Transaction> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.Transactions
            .AsNoTracking()
            .Include(t => t.Sender)
            .Include(t => t.Recipient)
            .OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
