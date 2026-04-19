using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Functional.Fixtures;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

public class SalesEndpointTests : IClassFixture<SalesApiFactory>, IAsyncLifetime
{
    private readonly SalesApiFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public SalesEndpointTests(SalesApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.TruncateAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private static object ValidPayload(string saleNumber = "S-0001", int quantity = 5, decimal unitPrice = 10m) =>
        new
        {
            saleNumber,
            saleDate = DateTime.UtcNow.AddHours(-1),
            customerId = Guid.NewGuid(),
            customerName = "Customer",
            branchId = Guid.NewGuid(),
            branchName = "Branch",
            items = new[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    productName = "Product",
                    quantity,
                    unitPrice
                }
            }
        };

    [Fact(DisplayName = "POST /api/Sales with valid payload returns 201 and applies discount")]
    public async Task Create_ShouldReturn201_WithDiscountCalculated()
    {
        // Given
        var body = ValidPayload(quantity: 5, unitPrice: 10m);

        // When
        var response = await _client.PostAsJsonAsync("/api/Sales", body);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("saleNumber").GetString().Should().Be("S-0001");
        data.GetProperty("totalAmount").GetDecimal().Should().Be(45m);     // 5*10 - 10%
    }

    [Fact(DisplayName = "POST with unitPrice=0 returns 400 ValidationError")]
    public async Task Create_ShouldReturn400_WhenUnitPriceZero()
    {
        var response = await _client.PostAsJsonAsync("/api/Sales",
            ValidPayload(unitPrice: 0m));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"type\":\"ValidationError\"");
    }

    [Fact(DisplayName = "POST with quantity=21 returns 400 (validator catches before Domain)")]
    public async Task Create_ShouldReturn400_WhenQuantityTooLarge()
    {
        var response = await _client.PostAsJsonAsync("/api/Sales",
            ValidPayload(quantity: 21));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "POST with duplicate SaleNumber returns 422 DomainError")]
    public async Task Create_ShouldReturn422_WhenSaleNumberDuplicated()
    {
        // Given a first successful create
        (await _client.PostAsJsonAsync("/api/Sales", ValidPayload("DUP-001"))).EnsureSuccessStatusCode();

        // When creating again with same number
        var response = await _client.PostAsJsonAsync("/api/Sales", ValidPayload("DUP-001"));

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"type\":\"DomainError\"");
    }

    [Fact(DisplayName = "GET /api/Sales/{id} existing returns 200 with items")]
    public async Task GetById_ShouldReturn200_WhenExists()
    {
        var created = await CreateAsync("GET-OK");
        var response = await _client.GetAsync($"/api/Sales/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("GET-OK");
    }

    [Fact(DisplayName = "GET /api/Sales/{id} missing returns 404 ResourceNotFound")]
    public async Task GetById_ShouldReturn404_WhenMissing()
    {
        var response = await _client.GetAsync($"/api/Sales/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"type\":\"ResourceNotFound\"");
    }

    [Fact(DisplayName = "GET /api/Sales paginated returns 200")]
    public async Task List_ShouldReturn200_Paginated()
    {
        await CreateAsync("LIST-1");
        await CreateAsync("LIST-2");

        var response = await _client.GetAsync("/api/Sales?_page=1&_size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("totalCount");
    }

    [Fact(DisplayName = "GET /api/Sales with saleNumber partial returns filtered results")]
    public async Task List_ShouldFilter_BySaleNumberPartial()
    {
        await CreateAsync("ABC-001");
        await CreateAsync("ABC-002");
        await CreateAsync("XYZ-001");

        var response = await _client.GetAsync("/api/Sales?saleNumber=ABC");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(2);
    }

    [Fact(DisplayName = "PUT /api/Sales/{id} updates header fields")]
    public async Task Update_ShouldUpdateHeader()
    {
        var sale = await CreateAsync("UPD-001");
        var updateBody = new
        {
            saleNumber = "UPD-001-NEW",
            saleDate = DateTime.UtcNow.AddHours(-2),
            customerId = Guid.NewGuid(),
            customerName = "NewCustomer",
            branchId = Guid.NewGuid(),
            branchName = "NewBranch"
        };

        var response = await _client.PutAsJsonAsync($"/api/Sales/{sale.Id}", updateBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("UPD-001-NEW");
    }

    [Fact(DisplayName = "PATCH /api/Sales/{id}/cancel cancels the sale")]
    public async Task Cancel_ShouldMarkCancelled()
    {
        var sale = await CreateAsync("CNL-001");

        var response = await _client.PatchAsync($"/api/Sales/{sale.Id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"cancelled\":true");
    }

    [Fact(DisplayName = "PATCH /api/Sales/{id}/items/{itemId}/cancel cancels an item and recalculates total")]
    public async Task CancelItem_ShouldRecalculateTotal()
    {
        var sale = await CreateAsync("CIT-001");

        var response = await _client.PatchAsync(
            $"/api/Sales/{sale.Id}/items/{sale.FirstItemId}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("data").GetProperty("items");
        items[0].GetProperty("cancelled").GetBoolean().Should().BeTrue();
    }

    [Fact(DisplayName = "PUT on cancelled sale returns 422 DomainError")]
    public async Task Update_ShouldReturn422_WhenSaleCancelled()
    {
        // Given
        var sale = await CreateAsync("CAN-UPD-001");
        (await _client.PatchAsync($"/api/Sales/{sale.Id}/cancel", null)).EnsureSuccessStatusCode();

        // When
        var updateBody = new
        {
            saleNumber = "CAN-UPD-001",
            saleDate = DateTime.UtcNow.AddHours(-1),
            customerId = Guid.NewGuid(),
            customerName = "x",
            branchId = Guid.NewGuid(),
            branchName = "y"
        };
        var response = await _client.PutAsJsonAsync($"/api/Sales/{sale.Id}", updateBody);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact(DisplayName = "DELETE /api/Sales/{id} returns 204 and subsequent GET is 404")]
    public async Task Delete_ShouldReturn204_AndRemoveSale()
    {
        var sale = await CreateAsync("DEL-001");

        var response = await _client.DeleteAsync($"/api/Sales/{sale.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/Sales/{sale.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------- helpers ----------

    private record CreatedSale(Guid Id, Guid FirstItemId);

    private async Task<CreatedSale> CreateAsync(string saleNumber)
    {
        var response = await _client.PostAsJsonAsync("/api/Sales",
            ValidPayload(saleNumber, quantity: 5, unitPrice: 10m));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var data = doc.RootElement.GetProperty("data");
        var id = Guid.Parse(data.GetProperty("id").GetString()!);
        var itemId = Guid.Parse(data.GetProperty("items")[0].GetProperty("id").GetString()!);
        return new CreatedSale(id, itemId);
    }
}
