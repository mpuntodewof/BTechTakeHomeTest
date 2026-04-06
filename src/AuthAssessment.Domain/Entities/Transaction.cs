namespace AuthAssessment.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid? IdempotencyKey { get; set; }
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Sender { get; set; } = default!;
    public User Recipient { get; set; } = default!;
}
