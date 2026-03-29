using AuthAssessment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthAssessment.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.Notes)
            .HasMaxLength(500);

        builder.HasOne(t => t.Sender)
            .WithMany(u => u.SentTransactions)
            .HasForeignKey(t => t.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Recipient)
            .WithMany(u => u.ReceivedTransactions)
            .HasForeignKey(t => t.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.SenderId);
        builder.HasIndex(t => t.RecipientId);
        builder.HasIndex(t => t.CreatedAt);
    }
}
