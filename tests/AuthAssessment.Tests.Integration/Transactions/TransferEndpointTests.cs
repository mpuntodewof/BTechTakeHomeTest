using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AuthAssessment.Application.DTOs.Transactions;
using AuthAssessment.Tests.Integration.Fixtures;
using AuthAssessment.Tests.Integration.Helpers;
using FluentAssertions;

namespace AuthAssessment.Tests.Integration.Transactions;

public class TransferEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TransferEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_Return200_OnSuccessfulTransfer()
    {
        // Arrange
        var client = _factory.CreateClient();
        var senderEmail = TestHelper.GenerateUniqueEmail("sender");
        var recipientEmail = TestHelper.GenerateUniqueEmail("recipient");

        // Register both users
        var senderAuth = await TestHelper.RegisterUserAsync(client, senderEmail, "Password123!", "Sender User");
        await TestHelper.RegisterUserAsync(client, recipientEmail, "Password123!", "Recipient User");

        // Authenticate as sender
        TestHelper.CreateAuthenticatedClient(client, senderAuth.AccessToken);

        var transferRequest = new TransferRequest
        {
            RecipientEmail = recipientEmail,
            Amount = 100.50m,
            Notes = "Test transfer"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/transactions/transfer", transferRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var transaction = JsonSerializer.Deserialize<TransactionResponse>(content, JsonOptions);

        transaction.Should().NotBeNull();
        transaction!.SenderEmail.Should().Be(senderEmail);
        transaction.RecipientEmail.Should().Be(recipientEmail);
        transaction.Amount.Should().Be(100.50m);
        transaction.Notes.Should().Be("Test transfer");
    }

    [Fact]
    public async Task Should_Return400_OnInsufficientBalance()
    {
        // Arrange
        var client = _factory.CreateClient();
        var senderEmail = TestHelper.GenerateUniqueEmail("brokesender");
        var recipientEmail = TestHelper.GenerateUniqueEmail("richrecipient");

        var senderAuth = await TestHelper.RegisterUserAsync(client, senderEmail, "Password123!", "Broke Sender");
        await TestHelper.RegisterUserAsync(client, recipientEmail, "Password123!", "Rich Recipient");

        TestHelper.CreateAuthenticatedClient(client, senderAuth.AccessToken);

        var transferRequest = new TransferRequest
        {
            RecipientEmail = recipientEmail,
            Amount = 999_999m, // More than the default 10,000 balance
            Notes = "This should fail"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/transactions/transfer", transferRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return401_WhenNotAuthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();

        var transferRequest = new TransferRequest
        {
            RecipientEmail = "someone@example.com",
            Amount = 50m,
            Notes = "Unauthenticated transfer"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/transactions/transfer", transferRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
