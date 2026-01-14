using MediatR;
using VeniceOrders.Application.DTOs;
using VeniceOrders.Application.Interfaces;
using VeniceOrders.Domain.Interfaces;

namespace VeniceOrders.Application.Queries.GetOrderById;

/// <summary>
/// Handler para buscar pedido por ID
/// Requisitos atendidos:
/// - Retornar dados vindos dos dois bancos integrados (SQL + Mongo)
/// - Cache Redis por 2 minutos
/// </summary>
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponseDto?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly ICacheService _cacheService;

    private const int CacheExpirationMinutes = 2;

    public GetOrderByIdQueryHandler(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        ICacheService cacheService)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _cacheService = cacheService;
    }

    public async Task<OrderResponseDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"order:{request.OrderId}";

        var cachedOrder = await _cacheService.GetAsync<OrderResponseDto>(cacheKey, cancellationToken);
        if (cachedOrder is not null)
        {
            return cachedOrder;
        }

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var items = await _orderItemRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        var response = new OrderResponseDto
        {
            Id = order.Id,
            ClienteId = order.ClienteId,
            Data = order.Data,
            Status = order.Status.ToString(),
            Total = order.Total,
            Itens = items.Select(item => new OrderItemResponseDto
            {
                Id = item.Id,
                Produto = item.Produto,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario,
                Subtotal = item.Subtotal
            }).ToList()
        };

        await _cacheService.SetAsync(
            cacheKey,
            response,
            TimeSpan.FromMinutes(CacheExpirationMinutes),
            cancellationToken);

        return response;
    }
}
