using AuthAssessment.Domain.Entities;

namespace AuthAssessment.Application.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken ct = default);
    Task<bool> ExistsByIdempotencyKeyAsync(Guid idempotencyKey, CancellationToken ct = default);
    Task<(List<Transaction> Items, int TotalCount)> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<(List<Transaction> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
}
