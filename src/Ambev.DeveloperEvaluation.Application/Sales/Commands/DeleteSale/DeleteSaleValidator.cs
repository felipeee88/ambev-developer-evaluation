using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.DeleteSale;

public class DeleteSaleValidator : AbstractValidator<DeleteSaleCommand>
{
    public DeleteSaleValidator()
    {
        RuleFor(x => x.Id).NotEqual(Guid.Empty);
    }
}
