using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AuthAssessment.Application.DTOs.Transactions;
using AuthAssessment.Tests.Integration.Fixtures;
using AuthAssessment.Tests.Integration.Helpers;
using FluentAssertions;

namespace AuthAssessment.Tests.Integration.Transactions;

public class HistoryEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public HistoryEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_ReturnPaginatedHistory_ForUser()
    {
        // Arrange
        var client = _factory.CreateClient();
        var senderEmail = TestHelper.GenerateUniqueEmail("historysender");
        var recipientEmail = TestHelper.GenerateUniqueEmail("historyrecipient");

        var senderAuth = await TestHelper.RegisterUserAsync(client, senderEmail, "Password123!", "History Sender");
        await TestHelper.RegisterUserAsync(client, recipientEmail, "Password123!", "History Recipient");

        TestHelper.CreateAuthenticatedClient(client, senderAuth.AccessToken);

        // Make a transfer to create history
        var transferRequest = new TransferRequest
        {
            RecipientEmail = recipientEmail,
            Amount = 50m,
            Notes = "History test transfer"
        };
        var transferResponse = await client.PostAsJsonAsync("/api/transactions/transfer", transferRequest);
        transferResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var response = await client.GetAsync("/api/transactions?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var history = JsonSerializer.Deserialize<TransactionListResponse>(content, JsonOptions);

        history.Should().NotBeNull();
        history!.Items.Should().NotBeEmpty();
        history.TotalCount.Should().BeGreaterThanOrEqualTo(1);
        history.Page.Should().Be(1);
        history.PageSize.Should().Be(10);

        var firstItem = history.Items.First();
        firstItem.SenderEmail.Should().Be(senderEmail);
        firstItem.RecipientEmail.Should().Be(recipientEmail);
        firstItem.Amount.Should().Be(50m);
    }
}
