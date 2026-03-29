using AuthAssessment.Domain.Entities;
using AuthAssessment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuthAssessment.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Email = "admin@authassessment.com",
            PasswordHash = "$2a$11$PmixqSgTA.5lktVendFxfO97LAAJ9dK11iAfgoeAavOPMUSHZAsi6",
            FullName = "System Admin",
            Balance = 10_000m,
            Role = UserRole.Admin,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
