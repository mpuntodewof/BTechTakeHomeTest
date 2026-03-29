using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AuthAssessment.Application.DTOs.Auth;
using AuthAssessment.Tests.Integration.Fixtures;
using AuthAssessment.Tests.Integration.Helpers;
using FluentAssertions;

namespace AuthAssessment.Tests.Integration.Auth;

public class RegisterEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RegisterEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Return201_AndTokens_OnValidRegistration()
    {
        // Arrange
        var email = TestHelper.GenerateUniqueEmail("register");
        var request = new RegisterRequest
        {
            Email = email,
            Password = "StrongPassword123!",
            ConfirmPassword = "StrongPassword123!",
            FullName = "Integration Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, JsonOptions);

        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
        authResponse.User.Should().NotBeNull();
        authResponse.User.Email.Should().Be(email);
        authResponse.User.FullName.Should().Be("Integration Test User");
        authResponse.User.Balance.Should().Be(10_000m);
    }

    [Fact]
    public async Task Should_Return400_WhenEmailTaken()
    {
        // Arrange
        var email = TestHelper.GenerateUniqueEmail("duplicate");
        var request = new RegisterRequest
        {
            Email = email,
            Password = "StrongPassword123!",
            ConfirmPassword = "StrongPassword123!",
            FullName = "First User"
        };

        // Register the first user
        var firstResponse = await _client.PostAsJsonAsync("/api/auth/register", request);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - try to register with the same email
        var secondRequest = new RegisterRequest
        {
            Email = email,
            Password = "AnotherPassword456!",
            ConfirmPassword = "AnotherPassword456!",
            FullName = "Second User"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
