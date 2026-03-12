using OrderProcessingApp.Api.Entities;
using OrderProcessingApp.Api.Dtos;

namespace OrderProcessingApp.Api.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(CreateOrderRequest request);
    Task<Order> GetOrderByIdAsync(string orderId);
    Task<Order> TransitionOrderStatusAsync(string orderId, StatusTransitionRequest request);
}
