using AuthAssessment.Application.Interfaces;
using AuthAssessment.Application.UseCases;
using AuthAssessment.Domain.Entities;
using AuthAssessment.Domain.Enums;
using FluentAssertions;
using Moq;

namespace AuthAssessment.Tests.Unit.UseCases;

public class UserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly UserUseCase _sut;

    public UserUseCaseTests()
    {
        _sut = new UserUseCase(_userRepository.Object);
    }

    #region GetProfile Tests

    [Fact]
    public async Task GetProfile_Should_ReturnProfile_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashed",
            FullName = "Test User",
            Balance = 5_000m,
            Role = UserRole.User,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        _userRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetProfileAsync(user.Id);

        // Assert
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be("test@example.com");
        result.FullName.Should().Be("Test User");
        result.Balance.Should().Be(5_000m);
        result.Role.Should().Be("User");
        result.CreatedAt.Should().Be(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task GetProfile_Should_Fail_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = () => _sut.GetProfileAsync(userId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*User not found*");
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_Should_ReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                PasswordHash = "hashed",
                FullName = "Admin User",
                Balance = 50_000m,
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                PasswordHash = "hashed",
                FullName = "Regular User",
                Balance = 10_000m,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            }
        };

        _userRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items[0].Email.Should().Be("admin@example.com");
        result.Items[0].Role.Should().Be("Admin");
        result.Items[1].Email.Should().Be("user@example.com");
        result.Items[1].Role.Should().Be("User");
    }

    #endregion
}
