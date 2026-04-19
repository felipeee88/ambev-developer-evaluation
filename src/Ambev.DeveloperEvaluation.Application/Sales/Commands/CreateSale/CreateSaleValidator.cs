using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale;

public class CreateSaleValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidator()
    {
        RuleFor(x => x.SaleNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SaleDate)
            .LessThanOrEqualTo(_ => DateTime.UtcNow)
            .WithMessage("SaleDate cannot be in the future.");

        RuleFor(x => x.CustomerId).NotEqual(Guid.Empty);
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.BranchId).NotEqual(Guid.Empty);
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Items).NotEmpty().WithMessage("Sale must have at least one item.");
        RuleForEach(x => x.Items).SetValidator(new CreateSaleItemInputValidator());
    }
}

public class CreateSaleItemInputValidator : AbstractValidator<CreateSaleItemInput>
{
    public CreateSaleItemInputValidator()
    {
        RuleFor(x => x.ProductId).NotEqual(Guid.Empty);
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Quantity).InclusiveBetween(1, 20);
        RuleFor(x => x.UnitPrice).GreaterThan(0m);
    }
}
