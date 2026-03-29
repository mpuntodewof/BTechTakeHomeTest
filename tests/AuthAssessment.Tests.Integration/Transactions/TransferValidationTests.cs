using System.Net;
using System.Net.Http.Json;
using AuthAssessment.Application.DTOs.Transactions;
using AuthAssessment.Tests.Integration.Fixtures;
using AuthAssessment.Tests.Integration.Helpers;
using FluentAssertions;

namespace AuthAssessment.Tests.Integration.Transactions;

public class TransferValidationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TransferValidationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_Return400_WhenTransferringToSelf()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = TestHelper.GenerateUniqueEmail("selftransfer");
        var auth = await TestHelper.RegisterUserAsync(client, email);
        TestHelper.CreateAuthenticatedClient(client, auth.AccessToken);

        var request = new TransferRequest
        {
            RecipientEmail = email,
            Amount = 100m,
            Notes = "Self transfer"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/transactions/transfer", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Return404_WhenRecipientDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = TestHelper.GenerateUniqueEmail("sender");
        var auth = await TestHelper.RegisterUserAsync(client, email);
        TestHelper.CreateAuthenticatedClient(client, auth.AccessToken);

        var request = new TransferRequest
        {
            RecipientEmail = "nonexistent@example.com",
            Amount = 50m
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/transactions/transfer", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task Should_Return400_WhenAmountIsInvalid(decimal amount)
    {
        // Arrange
        var client = _factory.CreateClient();
        var senderEmail = TestHelper.GenerateUniqueEmail("invalidsender");
        var recipientEmail = TestHelper.GenerateUniqueEmail("invalidrecipient");

        var auth = await TestHelper.RegisterUserAsync(client, senderEmail);
        await TestHelper.RegisterUserAsync(client, recipientEmail);
        TestHelper.CreateAuthenticatedClient(client, auth.AccessToken);

        var request = new TransferRequest
        {
            RecipientEmail = recipientEmail,
            Amount = amount
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/transactions/transfer", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_UpdateBalances_AfterSuccessfulTransfer()
    {
        // Arrange
        var client = _factory.CreateClient();
        var senderEmail = TestHelper.GenerateUniqueEmail("balancesender");
        var recipientEmail = TestHelper.GenerateUniqueEmail("balancerecipient");

        var senderAuth = await TestHelper.RegisterUserAsync(client, senderEmail);
        var recipientAuth = await TestHelper.RegisterUserAsync(client, recipientEmail);

        TestHelper.CreateAuthenticatedClient(client, senderAuth.AccessToken);

        // Act — transfer 2,500
        var transferRequest = new TransferRequest
        {
            RecipientEmail = recipientEmail,
            Amount = 2_500m,
            Notes = "Balance check"
        };
        var transferResponse = await client.PostAsJsonAsync("/api/transactions/transfer", transferRequest);
        transferResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert — check sender balance
        var senderProfile = await client.GetAsync("/api/users/me");
        var senderContent = await senderProfile.Content.ReadAsStringAsync();
        senderContent.Should().Contain("7500"); // 10000 - 2500

        // Check recipient balance by logging in as recipient
        var client2 = _factory.CreateClient();
        var recipientLogin = await TestHelper.LoginUserAsync(client2, recipientEmail);
        TestHelper.CreateAuthenticatedClient(client2, recipientLogin.AccessToken);

        var recipientProfile = await client2.GetAsync("/api/users/me");
        var recipientContent = await recipientProfile.Content.ReadAsStringAsync();
        recipientContent.Should().Contain("12500"); // 10000 + 2500
    }
}
