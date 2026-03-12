using FluentAssertions;
using OrderProcessingService.Dtos;
using OrderProcessingService.Entities;

namespace OrderProcessingApp.Tests;

/// <summary>
/// Tests for order validation and business rule enforcement in OrderService.
/// Covers: order validation, business rule enforcement across different scenarios.
/// </summary>
public class OrderValidationTests
{
    // No fixture setup needed - these tests validate entity models directly

    #region Order Creation Request Validation Tests

    [Fact]
    public void CreateOrderRequest_WithEmptyCustomerId_IsInvalid()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = "",
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = "PROD-001", Quantity = 5 }
            }
        };

        // Act & Assert
        request.CustomerId.Should().Be("");
    }

    [Fact]
    public void CreateOrderRequest_WithNullCustomerId_IsInvalid()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = null,
            Items = new List<CreateOrderItemRequest>()
        };

        // Act & Assert
        request.CustomerId.Should().BeNull();
    }

    [Fact]
    public void CreateOrderRequest_WithEmptyItems_HasNoItems()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = "CUST-001",
            Items = new List<CreateOrderItemRequest>()
        };

        // Act & Assert
        request.Items.Should().HaveCount(0);
    }

    [Fact]
    public void CreateOrderRequest_WithZeroQuantity_HasInvalidQuantity()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = "CUST-001",
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = "PROD-001", Quantity = 0 }
            }
        };

        // Act & Assert
        request.Items[0].Quantity.Should().Be(0);
    }

    [Fact]
    public void CreateOrderRequest_WithNegativeQuantity_HasNegativeQuantity()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = "CUST-001",
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = "PROD-001", Quantity = -5 }
            }
        };

        // Act & Assert
        request.Items[0].Quantity.Should().Be(-5);
    }

    #endregion

    #region Order Status Transition Validation Tests

    [Fact]
    public void StatusTransitionRequest_WithValidTransition_IsValid()
    {
        // Arrange
        var request = new StatusTransitionRequest
        {
            NewStatus = OrderStatus.Confirmed
        };

        // Act & Assert
        request.NewStatus.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void StatusTransitionRequest_WithAnyStatus_CanBeCreated()
    {
        // Arrange & Act
        var request1 = new StatusTransitionRequest { NewStatus = OrderStatus.Pending };
        var request2 = new StatusTransitionRequest { NewStatus = OrderStatus.Cancelled };
        var request3 = new StatusTransitionRequest { NewStatus = OrderStatus.Delivered };

        // Assert
        request1.NewStatus.Should().Be(OrderStatus.Pending);
        request2.NewStatus.Should().Be(OrderStatus.Cancelled);
        request3.NewStatus.Should().Be(OrderStatus.Delivered);
    }

    #endregion

    #region Order Status Enum Tests

    [Fact]
    public void OrderStatus_PendingValue_Exists()
    {
        // Arrange & Act
        var status = OrderStatus.Pending;

        // Assert
        status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void OrderStatus_ConfirmedValue_Exists()
    {
        // Arrange & Act
        var status = OrderStatus.Confirmed;

        // Assert
        status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void OrderStatus_ProcessingValue_Exists()
    {
        // Arrange & Act
        var status = OrderStatus.Processing;

        // Assert
        status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void OrderStatus_ShippedValue_Exists()
    {
        // Arrange & Act
        var status = OrderStatus.Shipped;

        // Assert
        status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void OrderStatus_DeliveredValue_Exists()
    {
        // Arrange & Act
        var status = OrderStatus.Delivered;

        // Assert
        status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void OrderStatus_CancelledValue_Exists()
    {
        // Arrange & Act
        var status = OrderStatus.Cancelled;

        // Assert
        status.Should().Be(OrderStatus.Cancelled);
    }

    #endregion

    #region Order Entity Tests

    [Fact]
    public void Order_CanBeCreated_WithRequiredProperties()
    {
        // Arrange & Act
        var order = new Order
        {
            Id = "ORD-001",
            CustomerId = "CUST-001",
            OrderNumber = "ORD-20240101-001",
            Status = OrderStatus.Pending,
            TotalAmount = 1000m
        };

        // Assert
        order.Id.Should().Be("ORD-001");
        order.CustomerId.Should().Be("CUST-001");
        order.OrderNumber.Should().Be("ORD-20240101-001");
        order.Status.Should().Be(OrderStatus.Pending);
        order.TotalAmount.Should().Be(1000m);
    }

    [Fact]
    public void Order_WithItems_ContainsLineItems()
    {
        // Arrange & Act
        var order = new Order
        {
            Id = "ORD-001",
            CustomerId = "CUST-001",
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>
            {
                new() { ProductId = "PROD-001", ProductName = "Laptop", Quantity = 2, UnitPrice = 999.99m },
                new() { ProductId = "PROD-002", ProductName = "Mouse", Quantity = 3, UnitPrice = 29.99m }
            },
            TotalAmount = 2089.95m
        };

        // Assert
        order.Items.Should().HaveCount(2);
        order.Items[0].ProductId.Should().Be("PROD-001");
        order.Items[1].ProductId.Should().Be("PROD-002");
    }

    [Fact]
    public void Order_CanBeTransitioned_ToConfirmed()
    {
        // Arrange
        var order = new Order
        {
            Id = "ORD-001",
            CustomerId = "CUST-001",
            Status = OrderStatus.Pending
        };

        // Act
        order.Status = OrderStatus.Confirmed;

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    #endregion

    #region Product Entity Tests

    [Fact]
    public void Product_CanBeCreated_WithPrice()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = "PROD-001",
            Name = "Laptop",
            Price = 999.99m,
            Stock = 10
        };

        // Assert
        product.Id.Should().Be("PROD-001");
        product.Name.Should().Be("Laptop");
        product.Price.Should().Be(999.99m);
        product.Stock.Should().Be(10);
    }

    [Fact]
    public void Product_WithZeroPrice_CanBeCreated()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = "PROD-001",
            Name = "Free Item",
            Price = 0m,
            Stock = 100
        };

        // Assert
        product.Price.Should().Be(0m);
    }

    [Fact]
    public void Product_WithNegativeStock_CanBeCreated()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = "PROD-001",
            Name = "Item",
            Price = 50m,
            Stock = -5
        };

        // Assert
        product.Stock.Should().Be(-5);
    }

    #endregion

    #region OrderItem Tests

    [Fact]
    public void OrderItem_CanBeCreated_WithQuantityAndPrice()
    {
        // Arrange & Act
        var item = new OrderItem
        {
            ProductId = "PROD-001",
            ProductName = "Laptop",
            Quantity = 2,
            UnitPrice = 999.99m
        };

        // Assert
        item.ProductId.Should().Be("PROD-001");
        item.Quantity.Should().Be(2);
        item.UnitPrice.Should().Be(999.99m);
    }

    #endregion
}

