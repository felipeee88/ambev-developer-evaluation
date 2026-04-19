using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed class SaleModifiedEvent : INotification
{
    public Guid SaleId { get; }
    public string SaleNumber { get; }
    public DateTime OccurredOn { get; }

    public SaleModifiedEvent(Guid saleId, string saleNumber)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        OccurredOn = DateTime.UtcNow;
    }
}
