using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed class ItemCancelledEvent : INotification
{
    public Guid SaleId { get; }
    public string SaleNumber { get; }
    public Guid ItemId { get; }
    public DateTime OccurredOn { get; }

    public ItemCancelledEvent(Guid saleId, string saleNumber, Guid itemId)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        ItemId = itemId;
        OccurredOn = DateTime.UtcNow;
    }
}
