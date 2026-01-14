using FluentAssertions;
using Moq;
using VeniceOrders.Application.DTOs;
using VeniceOrders.Application.Interfaces;
using VeniceOrders.Application.Queries.GetOrderById;
using VeniceOrders.Domain.Entities;
using VeniceOrders.Domain.Interfaces;

namespace VeniceOrders.UnitTests.Application;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
        _cacheServiceMock = new Mock<ICacheService>();

        _handler = new GetOrderByIdQueryHandler(
            _orderRepositoryMock.Object,
            _orderItemRepositoryMock.Object,
            _cacheServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_OrderExistsInCache_ShouldReturnCachedOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var cachedOrder = new OrderResponseDto
        {
            Id = orderId,
            ClienteId = Guid.NewGuid(),
            Status = "Pending",
            Total = 100.00m
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<OrderResponseDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedOrder);

        var query = new GetOrderByIdQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(cachedOrder);

        _orderRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_OrderNotInCache_ShouldFetchFromRepositoriesAndCache()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();

        _cacheServiceMock
            .Setup(x => x.GetAsync<OrderResponseDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderResponseDto?)null);

        var order = new Order(clienteId);
        typeof(Order).GetProperty("Id")!.SetValue(order, orderId);
        order.SetTotal(500.00m);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var orderItems = new List<OrderItem>
        {
            OrderItem.Reconstruct(Guid.NewGuid(), orderId, "Produto A", 2, 250.00m)
        };

        _orderItemRepositoryMock
            .Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderItems);

        var query = new GetOrderByIdQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.Total.Should().Be(500.00m);
        result.Itens.Should().HaveCount(1);

        _cacheServiceMock.Verify(
            x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<OrderResponseDto>(),
                It.Is<TimeSpan>(ts => ts.TotalMinutes == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldReturnNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _cacheServiceMock
            .Setup(x => x.GetAsync<OrderResponseDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderResponseDto?)null);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var query = new GetOrderByIdQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _orderItemRepositoryMock.Verify(
            x => x.GetByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _cacheServiceMock.Verify(
            x => x.SetAsync(It.IsAny<string>(), It.IsAny<OrderResponseDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
