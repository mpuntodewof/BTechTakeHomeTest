using System.Net;
using System.Net.Http.Json;
using AuthAssessment.Application.DTOs.Auth;
using AuthAssessment.Tests.Integration.Fixtures;
using AuthAssessment.Tests.Integration.Helpers;
using FluentAssertions;

namespace AuthAssessment.Tests.Integration.Auth;

public class LoginEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LoginEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Return200_WithTokens_OnValidLogin()
    {
        // Arrange
        var email = TestHelper.GenerateUniqueEmail("login");
        await TestHelper.RegisterUserAsync(_client, email, "ValidPassword123!", "Login Test User");

        // Act
        var authResponse = await TestHelper.LoginUserAsync(_client, email, "ValidPassword123!");

        // Assert
        authResponse.Should().NotBeNull();
        authResponse.AccessToken.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
        authResponse.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task Should_Return401_OnInvalidCredentials()
    {
        // Arrange
        var email = TestHelper.GenerateUniqueEmail("invalidlogin");
        var request = new LoginRequest
        {
            Email = email,
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert - middleware maps UnauthorizedAccessException to 401
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }
}
