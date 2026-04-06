using AuthAssessment.Application.DTOs.Transactions;
using AuthAssessment.Application.Interfaces;
using AuthAssessment.Domain.Entities;

namespace AuthAssessment.Application.UseCases;

public sealed class TransactionUseCase(
    IUserRepository userRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<TransactionResponse> TransferFundAsync(
        Guid senderId, TransferRequest request, CancellationToken ct = default)
    {
        // Idempotency check: reject duplicate transfers from offline queue retries
        if (request.IdempotencyKey.HasValue)
        {
            if (await transactionRepository.ExistsByIdempotencyKeyAsync(request.IdempotencyKey.Value, ct))
                throw new InvalidOperationException("This transfer has already been processed.");
        }

        var sender = await userRepository.GetByIdAsync(senderId, ct)
            ?? throw new InvalidOperationException("Sender not found.");

        var recipient = await userRepository.GetByEmailAsync(request.RecipientEmail.ToLowerInvariant(), ct)
            ?? throw new KeyNotFoundException("Recipient not found.");

        if (sender.Id == recipient.Id)
            throw new InvalidOperationException("Cannot transfer funds to yourself.");

        if (request.Amount <= 0)
            throw new InvalidOperationException("Transfer amount must be greater than zero.");

        if (sender.Balance < request.Amount)
            throw new InvalidOperationException("Insufficient balance.");

        sender.Balance -= request.Amount;
        recipient.Balance += request.Amount;

        userRepository.Update(sender);
        userRepository.Update(recipient);

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = request.IdempotencyKey,
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Amount = request.Amount,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await transactionRepository.AddAsync(transaction, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new TransactionResponse(
            transaction.Id,
            sender.Email,
            sender.FullName,
            recipient.Email,
            recipient.FullName,
            transaction.Amount,
            transaction.Notes,
            transaction.CreatedAt
        );
    }

    public async Task<TransactionListResponse> GetHistoryForUserAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, totalCount) = await transactionRepository.GetByUserIdAsync(userId, page, pageSize, ct);

        return new TransactionListResponse(
            items.Select(t => new TransactionResponse(
                t.Id, t.Sender.Email, t.Sender.FullName,
                t.Recipient.Email, t.Recipient.FullName,
                t.Amount, t.Notes, t.CreatedAt
            )).ToList(),
            totalCount, page, pageSize
        );
    }

    public async Task<TransactionListResponse> GetAllHistoryAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var (items, totalCount) = await transactionRepository.GetAllAsync(page, pageSize, ct);

        return new TransactionListResponse(
            items.Select(t => new TransactionResponse(
                t.Id, t.Sender.Email, t.Sender.FullName,
                t.Recipient.Email, t.Recipient.FullName,
                t.Amount, t.Notes, t.CreatedAt
            )).ToList(),
            totalCount, page, pageSize
        );
    }
}
