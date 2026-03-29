namespace AuthAssessment.Application.DTOs.Transactions;

public record TransactionResponse(
    Guid Id,
    string SenderEmail,
    string SenderName,
    string RecipientEmail,
    string RecipientName,
    decimal Amount,
    string? Notes,
    DateTime CreatedAt
);

public record TransactionListResponse(
    List<TransactionResponse> Items,
    int TotalCount,
    int Page,
    int PageSize
);
