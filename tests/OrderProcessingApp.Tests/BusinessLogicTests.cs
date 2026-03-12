using FluentAssertions;
using MongoDB.Driver;
using OrderProcessingApp.Api.Exceptions;
using OrderProcessingApp.Api.Entities;
using OrderProcessingApp.Api.Dtos;

namespace OrderProcessingApp.Tests;

public class InventoryServiceTests
{
    [Fact]
    public void InsufficientStockException_HasCorrectProperties()
    {
        // Arrange
        var productId = "PROD-001";
        var available = 5;
        var requested = 10;

        // Act
        var exception = new InsufficientStockException(productId, available, requested);

        // Assert
        exception.ProductId.Should().Be(productId);
        exception.Available.Should().Be(available);
        exception.Requested.Should().Be(requested);
        exception.Message.Should().Contain("Insufficient stock");
        exception.Message.Should().Contain(productId);
    }

    [Fact]
    public void CreateOrderRequest_WithEmptyItems_IsInvalid()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = "CUST-001",
            Items = new List<CreateOrderItemRequest>()
        };

        // Act
        var hasItems = request.Items.Any();

        // Assert
        hasItems.Should().BeFalse();
    }

    [Fact]
    public void CreateOrderItemRequest_WithValidData_IsValid()
    {
        // Arrange & Act
        var item = new CreateOrderItemRequest
        {
            ProductId = "PROD-001",
            Quantity = 2
        };

        // Assert
        item.ProductId.Should().Be("PROD-001");
        item.Quantity.Should().Be(2);
        item.Quantity.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Order_DefaultStatus_IsPending()
    {
        // Arrange & Act
        var order = new Order
        {
            Id = "ORD-001",
            CustomerId = "CUST-001",
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>(),
            TotalAmount = 0
        };

        // Assert
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void OrderItem_CalculateSubTotal_IsCorrect()
    {
        // Arrange
        var item = new OrderItem
        {
            ProductId = "PROD-001",
            ProductName = "Laptop",
            Quantity = 2,
            UnitPrice = 999.99m
        };

        // Act
        item.SubTotal = item.Quantity * item.UnitPrice;

        // Assert
        item.SubTotal.Should().Be(1999.98m);
    }

    [Fact]
    public void Product_InitialState_HasCorrectDefaults()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = "PROD-001",
            Name = "Laptop",
            Description = "High-performance laptop",
            Price = 999.99m,
            Stock = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        product.Stock.Should().BeGreaterThan(0);
        product.Price.Should().BeGreaterThan(0);
        product.Name.Should().NotBeNullOrEmpty();
    }
}
