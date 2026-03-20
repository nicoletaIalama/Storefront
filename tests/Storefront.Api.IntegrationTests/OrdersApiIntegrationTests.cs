using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Storefront.Api.IntegrationTests;

public sealed class OrdersApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrder_unknownId_returns_404_problem_details_with_detail()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/orders/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem!.Status);
        Assert.Contains(id.ToString(), problem.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateOrder_emptyLineItems_returns_400_problem_details_with_detail()
    {
        // Arrange
        var body = new { lineItems = Array.Empty<object>() };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem!.Status);
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));
    }

    [Fact]
    public async Task MockPayment_twice_secondCall_returns_400_predictable_state()
    {
        // Arrange
        var productId = Guid.Parse("a3f1c2d4-7b8e-4c1a-9f2d-5e6b7c8d9a01");
        var createBody = new
        {
            lineItems = new[] { new { productId, quantity = 1 } }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/orders", createBody);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = created.GetProperty("orderId").GetGuid();

        var payOk = await _client.PostAsJsonAsync(
            $"/api/orders/{orderId}/payments/mock",
            new { result = "success" });
        payOk.EnsureSuccessStatusCode();

        // Act
        var payAgain = await _client.PostAsJsonAsync(
            $"/api/orders/{orderId}/payments/mock",
            new { result = "success" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, payAgain.StatusCode);
        var problem = await payAgain.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem?.Detail);
        Assert.Contains("Paid", problem!.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Cannot process payment", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }
}
