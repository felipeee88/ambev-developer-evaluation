using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed class SaleCancelledEvent : INotification
{
    public Guid SaleId { get; }
    public string SaleNumber { get; }
    public DateTime OccurredOn { get; }

    public SaleCancelledEvent(Guid saleId, string saleNumber)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        OccurredOn = DateTime.UtcNow;
    }
}
