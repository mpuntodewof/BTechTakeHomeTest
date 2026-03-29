using System.Net;
using System.Text.Json;
using AuthAssessment.Application.DTOs.Users;
using AuthAssessment.Tests.Integration.Fixtures;
using AuthAssessment.Tests.Integration.Helpers;
using FluentAssertions;

namespace AuthAssessment.Tests.Integration.Users;

public class UsersEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public UsersEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_ReturnUserProfile_ForAuthenticatedUser()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = TestHelper.GenerateUniqueEmail("profile");
        var authResponse = await TestHelper.RegisterUserAsync(client, email, "Password123!", "Profile Test User");

        TestHelper.CreateAuthenticatedClient(client, authResponse.AccessToken);

        // Act
        var response = await client.GetAsync("/api/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var userProfile = JsonSerializer.Deserialize<UserResponse>(content, JsonOptions);

        userProfile.Should().NotBeNull();
        userProfile!.Email.Should().Be(email);
        userProfile.FullName.Should().Be("Profile Test User");
        userProfile.Balance.Should().Be(10_000m);
        userProfile.Role.Should().Be("User");
    }
}
