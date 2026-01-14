using FluentAssertions;
using Moq;
using VeniceOrders.Application.Commands.CreateOrder;
using VeniceOrders.Application.DTOs;
using VeniceOrders.Domain.Entities;
using VeniceOrders.Domain.Events;
using VeniceOrders.Domain.Interfaces;

namespace VeniceOrders.UnitTests.Application;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();

        _handler = new CreateOrderCommandHandler(
            _orderRepositoryMock.Object,
            _orderItemRepositoryMock.Object,
            _eventPublisherMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateOrderAndPublishEvent()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            ClienteId = Guid.NewGuid(),
            Itens = new List<OrderItemDto>
            {
                new() { Produto = "Notebook", Quantidade = 2, PrecoUnitario = 4500.00m },
                new() { Produto = "Mouse", Quantidade = 1, PrecoUnitario = 150.00m }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().NotBeEmpty();
        result.Message.Should().Be("Pedido criado com sucesso");

        _orderRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _orderItemRepositoryMock.Verify(
            x => x.AddManyAsync(It.Is<IEnumerable<OrderItem>>(items => items.Count() == 2), It.IsAny<CancellationToken>()),
            Times.Once);

        _eventPublisherMock.Verify(
            x => x.PublishAsync(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            ClienteId = Guid.NewGuid(),
            Itens = new List<OrderItemDto>
            {
                new() { Produto = "Produto A", Quantidade = 2, PrecoUnitario = 100.00m },
                new() { Produto = "Produto B", Quantidade = 3, PrecoUnitario = 50.00m }
            }
        };

        Order? capturedOrder = null;
        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedOrder.Should().NotBeNull();
        capturedOrder!.Total.Should().Be(350.00m);
    }
}
