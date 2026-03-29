using System.Net;
using System.Net.Http.Json;
using AuthAssessment.Application.DTOs.Auth;
using AuthAssessment.Tests.Integration.Fixtures;
using AuthAssessment.Tests.Integration.Helpers;
using FluentAssertions;

namespace AuthAssessment.Tests.Integration.Auth;

public class RegisterValidationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RegisterValidationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Return400_WhenEmailIsInvalid()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "not-an-email",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FullName = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_WhenPasswordTooShort()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = TestHelper.GenerateUniqueEmail("shortpw"),
            Password = "Ab1",
            ConfirmPassword = "Ab1",
            FullName = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_WhenConfirmPasswordDoesNotMatch()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = TestHelper.GenerateUniqueEmail("mismatch"),
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword456!",
            FullName = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_WhenFullNameIsMissing()
    {
        // Arrange — send raw JSON to bypass record default
        var payload = new { email = TestHelper.GenerateUniqueEmail("noname"), password = "Password123!", confirmPassword = "Password123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return400_WhenEmailIsMissing()
    {
        // Arrange
        var payload = new { password = "Password123!", confirmPassword = "Password123!", fullName = "Test User" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
