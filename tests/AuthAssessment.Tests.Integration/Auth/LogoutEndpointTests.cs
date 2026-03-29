using System.Net;
using System.Net.Http.Json;
using AuthAssessment.Application.DTOs.Auth;
using AuthAssessment.Tests.Integration.Fixtures;
using AuthAssessment.Tests.Integration.Helpers;
using FluentAssertions;

namespace AuthAssessment.Tests.Integration.Auth;

public class LogoutEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public LogoutEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_Return204_OnSuccessfulLogout()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = TestHelper.GenerateUniqueEmail("logout");
        var authResponse = await TestHelper.RegisterUserAsync(client, email);
        TestHelper.CreateAuthenticatedClient(client, authResponse.AccessToken);

        // Act
        var response = await client.PostAsync("/api/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Should_Return401_WhenNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_InvalidateRefreshToken_AfterLogout()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = TestHelper.GenerateUniqueEmail("logoutinvalidate");
        var authResponse = await TestHelper.RegisterUserAsync(client, email);
        var refreshToken = authResponse.RefreshToken;

        TestHelper.CreateAuthenticatedClient(client, authResponse.AccessToken);

        // Logout
        var logoutResponse = await client.PostAsync("/api/auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act — try to use the old refresh token
        var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshToken };
        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert — token should be revoked
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
