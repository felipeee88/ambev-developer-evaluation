using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.Integration.Fixtures;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

public class SaleRepositoryTests : IClassFixture<PostgresContainerFixture>, IAsyncLifetime
{
    private readonly PostgresContainerFixture _pg;

    public SaleRepositoryTests(PostgresContainerFixture pg) => _pg = pg;

    public Task InitializeAsync() => _pg.TruncateAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private SaleRepository CreateRepo(out ORM.DefaultContext ctx)
    {
        ctx = _pg.CreateContext();
        return new SaleRepository(ctx);
    }

    private static Sale BuildSale(string saleNumber = "S-0001", int itemCount = 2)
    {
        var sale = new Sale(saleNumber, DateTime.UtcNow,
            new CustomerInfo(Guid.NewGuid(), "Customer"),
            new BranchInfo(Guid.NewGuid(), "Branch"));
        for (var i = 0; i < itemCount; i++)
            sale.AddItem(new ProductInfo(Guid.NewGuid(), $"P{i}"), 5, 10m);
        return sale;
    }

    [Fact(DisplayName = "AddAsync persists sale with items and owned VOs")]
    public async Task AddAsync_ShouldPersistSaleWithItemsAndOwnedVOs()
    {
        // Given
        var repo = CreateRepo(out var ctx);
        await using var _ = ctx;
        var sale = BuildSale();

        // When
        await repo.AddAsync(sale);

        // Then
        await using var ctx2 = _pg.CreateContext();
        var loaded = await new SaleRepository(ctx2).GetByIdAsync(sale.Id);
        loaded.Should().NotBeNull();
        loaded!.SaleNumber.Should().Be("S-0001");
        loaded.Items.Should().HaveCount(2);
        loaded.Customer.Name.Should().Be("Customer");
        loaded.Branch.Name.Should().Be("Branch");
    }

    [Fact(DisplayName = "GetBySaleNumberAsync finds by business key")]
    public async Task GetBySaleNumberAsync_ShouldFindBySaleNumber()
    {
        // Given
        var repo = CreateRepo(out var ctx);
        await using var _ = ctx;
        await repo.AddAsync(BuildSale("S-LOOKUP"));

        // When
        await using var ctx2 = _pg.CreateContext();
        var found = await new SaleRepository(ctx2).GetBySaleNumberAsync("S-LOOKUP");

        // Then
        found.Should().NotBeNull();
        found!.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "GetBySaleNumberAsync returns null when not found")]
    public async Task GetBySaleNumberAsync_ShouldReturnNull_WhenNotFound()
    {
        var repo = CreateRepo(out var ctx);
        await using var _ = ctx;

        var found = await repo.GetBySaleNumberAsync("DOES-NOT-EXIST");

        found.Should().BeNull();
    }

    [Fact(DisplayName = "UpdateAsync persists changes")]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Given
        var addCtx = _pg.CreateContext();
        await new SaleRepository(addCtx).AddAsync(BuildSale("S-UPD"));
        await addCtx.DisposeAsync();

        // When
        await using var ctx2 = _pg.CreateContext();
        var repo = new SaleRepository(ctx2);
        var sale = (await repo.GetBySaleNumberAsync("S-UPD"))!;
        sale.ChangeHeader("S-UPD-NEW", sale.SaleDate, sale.Customer, sale.Branch);
        await repo.UpdateAsync(sale);

        // Then
        await using var ctx3 = _pg.CreateContext();
        var reloaded = await new SaleRepository(ctx3).GetByIdAsync(sale.Id);
        reloaded!.SaleNumber.Should().Be("S-UPD-NEW");
    }

    [Fact(DisplayName = "DeleteAsync returns false when id not found")]
    public async Task DeleteAsync_ShouldReturnFalse_WhenSaleNotFound()
    {
        var repo = CreateRepo(out var ctx);
        await using var _ = ctx;

        var ok = await repo.DeleteAsync(Guid.NewGuid());

        ok.Should().BeFalse();
    }

    [Fact(DisplayName = "DeleteAsync cascades into SaleItems")]
    public async Task DeleteAsync_ShouldCascadeDeleteItems_WhenSaleExists()
    {
        // Given
        var addCtx = _pg.CreateContext();
        var addRepo = new SaleRepository(addCtx);
        var sale = BuildSale("S-DEL", itemCount: 2);
        await addRepo.AddAsync(sale);
        await addCtx.DisposeAsync();

        // When
        await using var ctx2 = _pg.CreateContext();
        var ok = await new SaleRepository(ctx2).DeleteAsync(sale.Id);

        // Then
        ok.Should().BeTrue();
        await using var ctx3 = _pg.CreateContext();
        (await new SaleRepository(ctx3).GetByIdAsync(sale.Id)).Should().BeNull();
        // Direct table check — should be 0 items
        var count = ctx3.Set<SaleItem>().Count();
        count.Should().Be(0);
    }

    [Fact(DisplayName = "ListAsync applies CustomerId filter")]
    public async Task ListAsync_ShouldApplyCustomerIdFilter()
    {
        // Given
        await using var setup = _pg.CreateContext();
        var repoSetup = new SaleRepository(setup);
        var s1 = BuildSale("S-A", 1);
        var s2 = BuildSale("S-B", 1);
        await repoSetup.AddAsync(s1);
        await repoSetup.AddAsync(s2);

        // When
        await using var ctx = _pg.CreateContext();
        var result = await new SaleRepository(ctx).ListAsync(new SaleListQuery
        {
            CustomerId = s1.Customer.Id,
            Page = 1,
            PageSize = 10
        });

        // Then
        result.TotalItems.Should().Be(1);
        result.Data.Single().SaleNumber.Should().Be("S-A");
    }

    [Fact(DisplayName = "ListAsync applies saleNumber partial (ILIKE) filter")]
    public async Task ListAsync_ShouldApplySaleNumberPartialFilter()
    {
        // Given
        await using var setup = _pg.CreateContext();
        var repoSetup = new SaleRepository(setup);
        await repoSetup.AddAsync(BuildSale("ABC-001", 1));
        await repoSetup.AddAsync(BuildSale("ABC-002", 1));
        await repoSetup.AddAsync(BuildSale("XYZ-001", 1));

        // When
        await using var ctx = _pg.CreateContext();
        var result = await new SaleRepository(ctx).ListAsync(new SaleListQuery
        {
            SaleNumber = "abc",
            Page = 1,
            PageSize = 10
        });

        // Then
        result.TotalItems.Should().Be(2);
        result.Data.Select(s => s.SaleNumber).Should().BeEquivalentTo(new[] { "ABC-001", "ABC-002" });
    }

    [Fact(DisplayName = "ListAsync orders by totalAmount descending")]
    public async Task ListAsync_ShouldApplyOrderByTotalAmountDesc()
    {
        // Given
        await using var setup = _pg.CreateContext();
        var repoSetup = new SaleRepository(setup);
        var low = new Sale("LOW", DateTime.UtcNow,
            new CustomerInfo(Guid.NewGuid(), "c"), new BranchInfo(Guid.NewGuid(), "b"));
        low.AddItem(new ProductInfo(Guid.NewGuid(), "p"), 1, 10m);   // total 10
        var high = new Sale("HIGH", DateTime.UtcNow,
            new CustomerInfo(Guid.NewGuid(), "c"), new BranchInfo(Guid.NewGuid(), "b"));
        high.AddItem(new ProductInfo(Guid.NewGuid(), "p"), 10, 10m); // total 80
        await repoSetup.AddAsync(low);
        await repoSetup.AddAsync(high);

        // When
        await using var ctx = _pg.CreateContext();
        var result = await new SaleRepository(ctx).ListAsync(new SaleListQuery
        {
            Order = "totalAmount desc",
            Page = 1,
            PageSize = 10
        });

        // Then
        result.Data.Select(s => s.SaleNumber).Should().Equal("HIGH", "LOW");
    }

    [Fact(DisplayName = "ListAsync returns pagination metadata")]
    public async Task ListAsync_ShouldReturnPaginationMetadata()
    {
        // Given 12 sales
        await using var setup = _pg.CreateContext();
        var repoSetup = new SaleRepository(setup);
        for (var i = 0; i < 12; i++)
            await repoSetup.AddAsync(BuildSale($"S-{i:D3}", 1));

        // When
        await using var ctx = _pg.CreateContext();
        var result = await new SaleRepository(ctx).ListAsync(new SaleListQuery
        {
            Page = 2,
            PageSize = 5
        });

        // Then
        result.TotalItems.Should().Be(12);
        result.CurrentPage.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.Data.Should().HaveCount(5);
    }
}
