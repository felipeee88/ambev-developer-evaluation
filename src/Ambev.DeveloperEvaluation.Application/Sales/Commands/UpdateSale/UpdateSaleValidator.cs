using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.UpdateSale;

public class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleValidator()
    {
        RuleFor(x => x.Id).NotEqual(Guid.Empty);
        RuleFor(x => x.SaleNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SaleDate)
            .LessThanOrEqualTo(_ => DateTime.UtcNow)
            .WithMessage("SaleDate cannot be in the future.");
        RuleFor(x => x.CustomerId).NotEqual(Guid.Empty);
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BranchId).NotEqual(Guid.Empty);
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(200);
    }
}
