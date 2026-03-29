using AuthAssessment.Application.DTOs.Transactions;
using AuthAssessment.Application.Interfaces;
using AuthAssessment.Application.UseCases;
using AuthAssessment.Domain.Entities;
using AuthAssessment.Domain.Enums;
using FluentAssertions;
using Moq;

namespace AuthAssessment.Tests.Unit.UseCases;

public class TransactionUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<ITransactionRepository> _transactionRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly TransactionUseCase _sut;

    public TransactionUseCaseTests()
    {
        _sut = new TransactionUseCase(
            _userRepository.Object,
            _transactionRepository.Object,
            _unitOfWork.Object);
    }

    private static User CreateUser(string email, decimal balance = 10_000m) => new()
    {
        Id = Guid.NewGuid(),
        Email = email,
        PasswordHash = "hashed",
        FullName = email.Split('@')[0],
        Balance = balance,
        Role = UserRole.User,
        CreatedAt = DateTime.UtcNow
    };

    private static List<Transaction> CreateTransactions(User sender, User recipient, int count)
    {
        return Enumerable.Range(1, count).Select(i => new Transaction
        {
            Id = Guid.NewGuid(),
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Amount = 100m * i,
            Notes = $"Transaction {i}",
            CreatedAt = DateTime.UtcNow,
            Sender = sender,
            Recipient = recipient
        }).ToList();
    }

    #region TransferFund Tests

    [Fact]
    public async Task TransferFund_Should_DebitSender_CreditRecipient_CreateRecord()
    {
        // Arrange
        var sender = CreateUser("sender@example.com", 5_000m);
        var recipient = CreateUser("recipient@example.com", 3_000m);
        var request = new TransferRequest { RecipientEmail = "recipient@example.com", Amount = 1_000m, Notes = "Test transfer" };

        _userRepository.Setup(r => r.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sender);
        _userRepository.Setup(r => r.GetByEmailAsync("recipient@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipient);

        // Act
        var result = await _sut.TransferFundAsync(sender.Id, request);

        // Assert
        sender.Balance.Should().Be(4_000m);
        recipient.Balance.Should().Be(4_000m);
        result.Amount.Should().Be(1_000m);
        result.SenderEmail.Should().Be("sender@example.com");
        result.RecipientEmail.Should().Be("recipient@example.com");
        result.Notes.Should().Be("Test transfer");

        _userRepository.Verify(r => r.Update(sender), Times.Once);
        _userRepository.Verify(r => r.Update(recipient), Times.Once);
        _transactionRepository.Verify(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransferFund_Should_Fail_WhenInsufficientBalance()
    {
        // Arrange
        var sender = CreateUser("sender@example.com", 100m);
        var recipient = CreateUser("recipient@example.com");
        var request = new TransferRequest { RecipientEmail = "recipient@example.com", Amount = 500m };

        _userRepository.Setup(r => r.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sender);
        _userRepository.Setup(r => r.GetByEmailAsync("recipient@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipient);

        // Act
        Func<Task> act = () => _sut.TransferFundAsync(sender.Id, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Insufficient balance*");
    }

    [Fact]
    public async Task TransferFund_Should_Fail_WhenRecipientNotFound()
    {
        // Arrange
        var sender = CreateUser("sender@example.com");
        var request = new TransferRequest { RecipientEmail = "nobody@example.com", Amount = 100m };

        _userRepository.Setup(r => r.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sender);
        _userRepository.Setup(r => r.GetByEmailAsync("nobody@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = () => _sut.TransferFundAsync(sender.Id, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Recipient not found*");
    }

    [Fact]
    public async Task TransferFund_Should_Fail_WhenSenderEqualsRecipient()
    {
        // Arrange
        var sender = CreateUser("sender@example.com");
        var request = new TransferRequest { RecipientEmail = "sender@example.com", Amount = 100m };

        _userRepository.Setup(r => r.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sender);
        _userRepository.Setup(r => r.GetByEmailAsync("sender@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(sender);

        // Act
        Func<Task> act = () => _sut.TransferFundAsync(sender.Id, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot transfer funds to yourself*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task TransferFund_Should_Fail_WhenAmountIsZeroOrNegative(decimal amount)
    {
        // Arrange
        var sender = CreateUser("sender@example.com");
        var recipient = CreateUser("recipient@example.com");
        var request = new TransferRequest { RecipientEmail = "recipient@example.com", Amount = amount };

        _userRepository.Setup(r => r.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sender);
        _userRepository.Setup(r => r.GetByEmailAsync("recipient@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipient);

        // Act
        Func<Task> act = () => _sut.TransferFundAsync(sender.Id, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public async Task TransferFund_Should_Fail_WhenSenderNotFound()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var request = new TransferRequest { RecipientEmail = "recipient@example.com", Amount = 100m };

        _userRepository.Setup(r => r.GetByIdAsync(senderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = () => _sut.TransferFundAsync(senderId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Sender not found*");
    }

    [Fact]
    public async Task TransferFund_Should_UpdateBothBalances_Correctly()
    {
        // Arrange
        var sender = CreateUser("sender@example.com", 5_000m);
        var recipient = CreateUser("recipient@example.com", 2_000m);
        var request = new TransferRequest { RecipientEmail = "recipient@example.com", Amount = 1_500m };

        _userRepository.Setup(r => r.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sender);
        _userRepository.Setup(r => r.GetByEmailAsync("recipient@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipient);

        // Act
        await _sut.TransferFundAsync(sender.Id, request);

        // Assert
        sender.Balance.Should().Be(3_500m);
        recipient.Balance.Should().Be(3_500m);
    }

    [Fact]
    public async Task TransferFund_Should_Fail_WhenExactBalance_IsInsufficient()
    {
        // Arrange — balance equals amount should succeed, but amount > balance should fail
        var sender = CreateUser("sender@example.com", 100m);
        var recipient = CreateUser("recipient@example.com");
        var request = new TransferRequest { RecipientEmail = "recipient@example.com", Amount = 100.01m };

        _userRepository.Setup(r => r.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sender);
        _userRepository.Setup(r => r.GetByEmailAsync("recipient@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipient);

        // Act
        Func<Task> act = () => _sut.TransferFundAsync(sender.Id, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Insufficient balance*");
    }

    [Fact]
    public async Task TransferFund_Should_Succeed_WhenAmountEqualsBalance()
    {
        // Arrange
        var sender = CreateUser("sender@example.com", 100m);
        var recipient = CreateUser("recipient@example.com", 0m);
        var request = new TransferRequest { RecipientEmail = "recipient@example.com", Amount = 100m };

        _userRepository.Setup(r => r.GetByIdAsync(sender.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sender);
        _userRepository.Setup(r => r.GetByEmailAsync("recipient@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipient);

        // Act
        var result = await _sut.TransferFundAsync(sender.Id, request);

        // Assert
        sender.Balance.Should().Be(0m);
        recipient.Balance.Should().Be(100m);
        result.Amount.Should().Be(100m);
    }

    #endregion

    #region GetHistory Tests

    [Fact]
    public async Task GetHistoryForUser_Should_ReturnUserTransactions()
    {
        // Arrange
        var sender = CreateUser("sender@example.com");
        var recipient = CreateUser("recipient@example.com");
        var transactions = CreateTransactions(sender, recipient, 2);

        _transactionRepository.Setup(r => r.GetByUserIdAsync(sender.Id, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((transactions, 2));

        // Act
        var result = await _sut.GetHistoryForUserAsync(sender.Id, 1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items[0].SenderEmail.Should().Be("sender@example.com");
        result.Items[0].RecipientEmail.Should().Be("recipient@example.com");
    }

    [Fact]
    public async Task GetHistoryForUser_Should_ReturnEmpty_WhenNoTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _transactionRepository.Setup(r => r.GetByUserIdAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Transaction>(), 0));

        // Act
        var result = await _sut.GetHistoryForUserAsync(userId, 1, 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAllHistory_Should_ReturnAllTransactions_ForAdmin()
    {
        // Arrange
        var sender = CreateUser("sender@example.com");
        var recipient = CreateUser("recipient@example.com");
        var transactions = CreateTransactions(sender, recipient, 3);

        _transactionRepository.Setup(r => r.GetAllAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((transactions, 3));

        // Act
        var result = await _sut.GetAllHistoryAsync(1, 20);

        // Assert
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    #endregion
}
