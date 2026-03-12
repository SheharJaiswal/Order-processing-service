using OrderProcessingService.Entities;

namespace OrderProcessingService.Interfaces;

public interface IEventPublisher
{
    Task PublishOrderCreatedEventAsync(Order order);
    Task PublishOrderStatusChangedEventAsync(string orderId, OrderStatus oldStatus, OrderStatus newStatus);
    Task PublishOrderCancelledEventAsync(string orderId, string customerId, string reason);
}
