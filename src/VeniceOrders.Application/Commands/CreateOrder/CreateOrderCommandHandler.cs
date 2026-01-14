using MediatR;
using VeniceOrders.Domain.Entities;
using VeniceOrders.Domain.Events;
using VeniceOrders.Domain.Interfaces;

namespace VeniceOrders.Application.Commands.CreateOrder;

/// <summary>
/// Handler para criação de pedido
/// Requisitos atendidos:
/// - Armazenar pedido no SQL Server
/// - Armazenar itens no MongoDB
/// - Publicar evento PedidoCriado na fila
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IEventPublisher _eventPublisher;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        IEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(request.ClienteId);

        var orderItems = request.Itens.Select(item => new OrderItem(
            order.Id,
            item.Produto,
            item.Quantidade,
            item.PrecoUnitario
        )).ToList();

        var total = orderItems.Sum(item => item.Subtotal);
        order.SetTotal(total);

        await _orderRepository.AddAsync(order, cancellationToken);

        await _orderItemRepository.AddManyAsync(orderItems, cancellationToken);

        var orderCreatedEvent = new OrderCreatedEvent(
            order.Id,
            order.ClienteId,
            order.Data,
            order.Total,
            orderItems.Count
        );

        await _eventPublisher.PublishAsync(orderCreatedEvent, cancellationToken);

        return new CreateOrderResponse
        {
            OrderId = order.Id,
            Message = "Pedido criado com sucesso"
        };
    }
}
