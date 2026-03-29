using System.Net;
using AuthAssessment.Tests.Integration.Fixtures;
using AuthAssessment.Tests.Integration.Helpers;
using FluentAssertions;

namespace AuthAssessment.Tests.Integration.Users;

public class UsersAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public UsersAuthorizationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProfile_Should_Return401_WhenNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllUsers_Should_Return401_WhenNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllUsers_Should_Return403_WhenNotAdmin()
    {
        // Arrange — register as a regular User (not Admin)
        var client = _factory.CreateClient();
        var email = TestHelper.GenerateUniqueEmail("regularuser");
        var auth = await TestHelper.RegisterUserAsync(client, email);
        TestHelper.CreateAuthenticatedClient(client, auth.AccessToken);

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTransactionsAll_Should_Return403_WhenNotAdmin()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = TestHelper.GenerateUniqueEmail("nonadmin");
        var auth = await TestHelper.RegisterUserAsync(client, email);
        TestHelper.CreateAuthenticatedClient(client, auth.AccessToken);

        // Act
        var response = await client.GetAsync("/api/transactions/all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTransactions_Should_Return401_WhenNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
