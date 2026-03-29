using System.ComponentModel.DataAnnotations;

namespace AuthAssessment.Application.DTOs.Transactions;

public record TransferRequest
{
    [Required, EmailAddress]
    public string RecipientEmail { get; init; } = default!;

    [Required, Range(0.01, 1_000_000)]
    public decimal Amount { get; init; }

    [StringLength(500)]
    public string? Notes { get; init; }
}
