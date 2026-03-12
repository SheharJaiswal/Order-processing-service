using OrderProcessingService.Dtos;
using OrderProcessingService.Entities;

namespace OrderProcessingService.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(CreateOrderRequest request);
    Task<Order> GetOrderByIdAsync(string orderId);
    Task<Order> TransitionOrderStatusAsync(string orderId, StatusTransitionRequest request);
}
