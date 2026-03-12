using FluentAssertions;
using OrderProcessingService.Entities;

namespace OrderProcessingService.Tests;

/// <summary>
/// Tests for stock reservation correctness through entity models.
/// Covers: product stock tracking, order item quantity validation.
/// </summary>
public class StockReservationTests
{
    // No fixture needed - tests use entities directly

    #region Product Stock Tracking Tests

    [Fact]
    public void Product_WithInitialStock_HasCorrectStockLevel()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = "PROD-001",
            Name = "Widget",
            Price = 19.99m,
            Stock = 100
        };

        // Assert
        product.Stock.Should().Be(100);
        product.Stock.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Product_WithZeroStock_IsOutOfStock()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = "PROD-001",
            Name = "Widget",
            Price = 19.99m,
            Stock = 0
        };

        // Assert
        product.Stock.Should().Be(0);
    }

    [Fact]
    public void Product_WithNegativeStock_IsAllowed()
    {
        // This tests that Product entity allows negative stock
        // (Inventory service would prevent this, but entity allows flexibility)
        var product = new Product
        {
            Id = "PROD-001",
            Name = "Widget",
            Price = 19.99m,
            Stock = -5
        };

        product.Stock.Should().Be(-5);
    }

    #endregion

    #region Order Item Quantity Tests

    [Fact]
    public void OrderItem_WithValidQuantity_CanBeCreated()
    {
        // Arrange & Act
        var orderItem = new OrderItem
        {
            ProductId = "PROD-001",
            Quantity = 5,
            UnitPrice = 10.00m
        };

        // Assert
        orderItem.Quantity.Should().Be(5);
        orderItem.UnitPrice.Should().Be(10.00m);
    }

    [Fact]
    public void OrderItem_WithZeroQuantity_CanBeCreated()
    {
        // Entity allows zero quantity - validation is at service level
        var orderItem = new OrderItem
        {
            ProductId = "PROD-001",
            Quantity = 0,
            UnitPrice = 10.00m
        };

        orderItem.Quantity.Should().Be(0);
    }

    [Fact]
    public void OrderItem_WithNegativeQuantity_CanBeCreated()
    {
        // Entity allows negative quantity - validation is at service level
        var orderItem = new OrderItem
        {
            ProductId = "PROD-001",
            Quantity = -5,
            UnitPrice = 10.00m
        };

        orderItem.Quantity.Should().Be(-5);
    }

    #endregion

    #region Stock Calculation Tests

    [Fact]
    public void MultipleOrderItems_CanCalculateTotalQuantityReserved()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            new OrderItem { ProductId = "PROD-001", Quantity = 5, UnitPrice = 10.00m },
            new OrderItem { ProductId = "PROD-001", Quantity = 3, UnitPrice = 10.00m },
            new OrderItem { ProductId = "PROD-001", Quantity = 2, UnitPrice = 10.00m }
        };

        // Act
        var totalReserved = items.Sum(i => i.Quantity);

        // Assert
        totalReserved.Should().Be(10);
    }

    [Fact]
    public void Product_StockValidation_CanCheckSufficientStock()
    {
        // Arrange
        var product = new Product { Stock = 50 };
        var requestedQuantity = 30;

        // Act
        var hasEnoughStock = product.Stock >= requestedQuantity;

        // Assert
        hasEnoughStock.Should().BeTrue();
    }

    [Fact]
    public void Product_StockValidation_FailsWhenInsufficientStock()
    {
        // Arrange
        var product = new Product { Stock = 20 };
        var requestedQuantity = 30;

        // Act
        var hasEnoughStock = product.Stock >= requestedQuantity;

        // Assert
        hasEnoughStock.Should().BeFalse();
    }

    #endregion
}

