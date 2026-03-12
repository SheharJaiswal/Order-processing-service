using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using OrderProcessingService.Entities;
using OrderProcessingService.Interfaces;
using OrderProcessingService.Events;

namespace OrderProcessingService.Services;

public class EventPublisher : IEventPublisher
{
    private readonly IConnection _connection;
    private readonly ILogger<EventPublisher> _logger;
    private const string ExchangeName = "order.events";

    public EventPublisher(IConnection connection, ILogger<EventPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
        _logger.LogInformation("EventPublisher initialized");
    }

    public async Task PublishOrderCreatedEventAsync(Order order)
    {
        var orderEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(item => new OrderItemEvent
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        await PublishEventAsync(orderEvent, "order.created");
    }

    public async Task PublishOrderStatusChangedEventAsync(string orderId, OrderStatus oldStatus, OrderStatus newStatus)
    {
        var statusEvent = new OrderStatusChangedEvent
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = newStatus
        };

        await PublishEventAsync(statusEvent, "order.status.changed");
    }

    public async Task PublishOrderCancelledEventAsync(string orderId, string customerId, string reason)
    {
        var cancelEvent = new OrderCancelledEvent
        {
            OrderId = orderId,
            CustomerId = customerId,
            Reason = reason
        };

        await PublishEventAsync(cancelEvent, "order.cancelled");
    }

    private async Task PublishEventAsync(OrderEvent orderEvent, string routingKey)
    {
        try
        {
            using var channel = await _connection.CreateChannelAsync();

            // Declare the exchange (topic type for routing flexibility)
            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false
            );

            var message = JsonSerializer.Serialize(orderEvent);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                MessageId = orderEvent.EventId,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation("Published event {EventType} with routing key {RoutingKey}. Event ID: {EventId}",
                orderEvent.EventType, routingKey, orderEvent.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} with routing key {RoutingKey}",
                orderEvent.EventType, routingKey);
            throw;
        }
    }
}
