using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AuthAssessment.Application.DTOs.Auth;
using AuthAssessment.Tests.Integration.Fixtures;
using AuthAssessment.Tests.Integration.Helpers;
using FluentAssertions;

namespace AuthAssessment.Tests.Integration.Auth;

public class RefreshEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RefreshEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Return200_WithNewTokenPair()
    {
        // Arrange - register a user first to get tokens
        var email = TestHelper.GenerateUniqueEmail("refresh");
        var registerResult = await TestHelper.RegisterUserAsync(_client, email, "Password123!", "Refresh Test User");

        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = registerResult.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(content, JsonOptions);

        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
        authResponse.User.Email.Should().Be(email);
    }
}
