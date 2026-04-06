using System.ComponentModel.DataAnnotations;

namespace AuthAssessment.Application.DTOs.Transactions;

public record TransferRequest
{
    /// <summary>
    /// Client-generated UUID to prevent duplicate transfers on retry.
    /// If null, the server generates one (backwards compatible).
    /// </summary>
    public Guid? IdempotencyKey { get; init; }

    [Required, EmailAddress]
    public string RecipientEmail { get; init; } = default!;

    [Required, Range(0.01, 1_000_000)]
    public decimal Amount { get; init; }

    [StringLength(500)]
    public string? Notes { get; init; }
}
