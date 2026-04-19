using Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale;
using Ambev.DeveloperEvaluation.Unit.Domain.Sales.TestData;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class CreateSaleValidatorTests
{
    private readonly CreateSaleValidator _sut = new();

    [Fact(DisplayName = "Given valid command When validating Then no errors")]
    public void Validator_ShouldPass_WhenCommandIsValid()
    {
        var result = _sut.TestValidate(SalesFaker.CreateSaleCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Given empty SaleNumber When validating Then fails")]
    public void Validator_ShouldFail_WhenSaleNumberEmpty()
    {
        var cmd = SalesFaker.CreateSaleCommand();
        cmd.SaleNumber = string.Empty;

        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.SaleNumber);
    }

    [Fact(DisplayName = "Given SaleDate in the future When validating Then fails")]
    public void Validator_ShouldFail_WhenSaleDateInFuture()
    {
        var cmd = SalesFaker.CreateSaleCommand();
        cmd.SaleDate = DateTime.UtcNow.AddDays(1);

        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.SaleDate);
    }

    [Fact(DisplayName = "Given empty items When validating Then fails")]
    public void Validator_ShouldFail_WhenItemsEmpty()
    {
        var cmd = SalesFaker.CreateSaleCommand();
        cmd.Items.Clear();

        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Theory(DisplayName = "Given quantity outside [1,20] When validating Then fails")]
    [InlineData(0)]
    [InlineData(21)]
    public void Validator_ShouldFail_WhenQuantityOutOfRange(int quantity)
    {
        var cmd = SalesFaker.CreateSaleCommand();
        cmd.Items[0].Quantity = quantity;

        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Theory(DisplayName = "Given non-positive UnitPrice When validating Then fails")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validator_ShouldFail_WhenUnitPriceNotPositive(decimal unitPrice)
    {
        var cmd = SalesFaker.CreateSaleCommand();
        cmd.Items[0].UnitPrice = unitPrice;

        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor("Items[0].UnitPrice");
    }

    [Fact(DisplayName = "Given empty CustomerId When validating Then fails")]
    public void Validator_ShouldFail_WhenCustomerIdEmpty()
    {
        var cmd = SalesFaker.CreateSaleCommand();
        cmd.CustomerId = Guid.Empty;

        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.CustomerId);
    }
}
