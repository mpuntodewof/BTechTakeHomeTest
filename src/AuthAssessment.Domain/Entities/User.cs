using AuthAssessment.Domain.Enums;

namespace AuthAssessment.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public decimal Balance { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();
    public ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();
}
