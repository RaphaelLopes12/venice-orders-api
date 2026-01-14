using MediatR;
using VeniceOrders.Application.DTOs;

namespace VeniceOrders.Application.Commands.CreateOrder;

/// <summary>
/// Command para criação de pedido
/// Requisito: "Receber um JSON com os dados do pedido"
/// </summary>
public class CreateOrderCommand : IRequest<CreateOrderResponse>
{
    public Guid ClienteId { get; set; }
    public List<OrderItemDto> Itens { get; set; } = new();
}

public class CreateOrderResponse
{
    public Guid OrderId { get; set; }
    public string Message { get; set; } = string.Empty;
}
