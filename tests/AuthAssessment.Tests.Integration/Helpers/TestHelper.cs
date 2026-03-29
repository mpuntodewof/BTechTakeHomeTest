using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AuthAssessment.Application.DTOs.Auth;

namespace AuthAssessment.Tests.Integration.Helpers;

public static class TestHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<AuthResponse> RegisterUserAsync(
        HttpClient client,
        string email = "testuser@example.com",
        string password = "Password123!",
        string fullName = "Test User")
    {
        var request = new RegisterRequest
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FullName = fullName
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AuthResponse>(content, JsonOptions)!;
    }

    public static async Task<AuthResponse> LoginUserAsync(
        HttpClient client,
        string email = "testuser@example.com",
        string password = "Password123!")
    {
        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AuthResponse>(content, JsonOptions)!;
    }

    public static HttpClient CreateAuthenticatedClient(
        HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public static string GenerateUniqueEmail(string prefix = "user")
    {
        return $"{prefix}_{Guid.NewGuid():N}@example.com";
    }
}
