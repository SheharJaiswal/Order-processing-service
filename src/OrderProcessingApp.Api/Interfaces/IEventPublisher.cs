using OrderProcessingApp.Api.Entities;

namespace OrderProcessingApp.Api.Interfaces;

public interface IEventPublisher
{
    Task PublishOrderCreatedEventAsync(Order order);
    Task PublishOrderStatusChangedEventAsync(string orderId, OrderStatus oldStatus, OrderStatus newStatus);
    Task PublishOrderCancelledEventAsync(string orderId, string customerId, string reason);
}
