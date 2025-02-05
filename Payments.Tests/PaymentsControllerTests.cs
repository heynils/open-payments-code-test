using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Payments.Tests;

public class PaymentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PaymentsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task InitiatePayment_ReturnsCreated()
    {
        var request = new PaymentRequest
        {
            DebtorAccount = "DE0123456789",
            CreditorAccount = "SE0123456789",
            InstructedAmount= "100.50",
            Currency = "EUR"
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/payments")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("Client-ID", "Client-ID-1");

        var response = await _client.SendAsync(httpRequest);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task InitiatePayment_MissingClientID_ReturnsBadRequest()
    {
        var request = new PaymentRequest
        {
            DebtorAccount = "DE0123456789",
            CreditorAccount = "SE0123456789",
            InstructedAmount= "100.50",
            Currency = "EUR"
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/payments")
        {
            Content = JsonContent.Create(request)
        };

        var response = await _client.SendAsync(httpRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InitiatePayment_InvalidIban_ReturnsBadRequest()
    {
        var request = new PaymentRequest
        {
            DebtorAccount = "@DE0123456789",
            CreditorAccount = "SE0123456789",
            InstructedAmount= "100.50",
            Currency = "EUR"
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/payments")
        {
            Content = JsonContent.Create(request)
        };

        var response = await _client.SendAsync(httpRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InitiatePayment_InvalidAmount_ReturnsBadRequest()
    {
        var request = new PaymentRequest
        {
            DebtorAccount = "DE0123456789",
            CreditorAccount = "SE0123456789",
            InstructedAmount= "100.",
            Currency = "EUR"
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/payments")
        {
            Content = JsonContent.Create(request)
        };

        var response = await _client.SendAsync(httpRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InitiatePayment_CanHandleParallelRequests()
    {
        var request = new PaymentRequest
        {
            DebtorAccount = "DE0123456789",
            CreditorAccount = "SE0123456789",
            InstructedAmount= "100.50",
            Currency = "EUR"
        };

        var httpRequest1 = new HttpRequestMessage(HttpMethod.Post, "/payments")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest1.Headers.Add("Client-ID", "Client-ID-10");

        var httpRequest2 = new HttpRequestMessage(HttpMethod.Post, "/payments")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest2.Headers.Add("Client-ID", "Client-ID-20");

        var response1 = await _client.SendAsync(httpRequest1);
        var response2 = await _client.SendAsync(httpRequest2);

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
    }

    [Fact]
    public async Task InitiatePayment_ConflictOnDuplicatePayment()
    {
        var request = new PaymentRequest
        {
            DebtorAccount = "DE0123456789",
            CreditorAccount = "SE0123456789",
            InstructedAmount= "100.50",
            Currency = "EUR"
        };

        var httpRequest1 = new HttpRequestMessage(HttpMethod.Post, "/payments")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest1.Headers.Add("Client-ID", "Client-ID-2");

        var httpRequest2 = new HttpRequestMessage(HttpMethod.Post, "/payments")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest2.Headers.Add("Client-ID", "Client-ID-2");

        var response1 = await _client.SendAsync(httpRequest1);
        var response2 = await _client.SendAsync(httpRequest2);

        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
    }

    [Fact]
    public async Task GetTransactions_ReturnsTransactions()
    {
        var iban ="NO0123456789";
        var request = new PaymentRequest
        {
            DebtorAccount = "DE0123456789",
            CreditorAccount = iban,
            InstructedAmount= "100.50",
            Currency = "EUR"
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/payments")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Add("Client-ID", "Client-ID-30");

        await _client.SendAsync(httpRequest);

        await Task.Delay(TimeSpan.FromSeconds(4));

        var response = await _client.GetAsync($"/accounts/{iban}/transactions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var transactions = await response.Content.ReadFromJsonAsync<List<TransactionRespone>>();

        Assert.NotNull(transactions);
        Assert.Single(transactions);

        Assert.Equal(request.DebtorAccount, transactions.First().DebtorAccount);
        Assert.Equal(decimal.Parse(request.InstructedAmount), transactions.First().TransactionAmount);
    }

    [Fact]
    public async Task GetTransactions_ReturnsNoContent()
    {
        var iban ="FI0123456789";

        var response = await _client.GetAsync($"/accounts/{iban}/transactions");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
