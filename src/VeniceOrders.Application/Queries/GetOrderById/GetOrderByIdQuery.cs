using MediatR;
using VeniceOrders.Application.DTOs;

namespace VeniceOrders.Application.Queries.GetOrderById;

/// <summary>
/// Query para buscar pedido por ID
/// Requisito: "Endpoint GET /pedidos/{id}"
/// </summary>
public class GetOrderByIdQuery : IRequest<OrderResponseDto?>
{
    public Guid OrderId { get; set; }

    public GetOrderByIdQuery(Guid orderId)
    {
        OrderId = orderId;
    }
}
