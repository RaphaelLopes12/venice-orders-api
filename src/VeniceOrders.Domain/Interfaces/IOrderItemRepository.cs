using VeniceOrders.Domain.Entities;

namespace VeniceOrders.Domain.Interfaces;

/// <summary>
/// Repositório para OrderItem - implementado com MongoDB
/// </summary>
public interface IOrderItemRepository
{
    Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddManyAsync(IEnumerable<OrderItem> items, CancellationToken cancellationToken = default);
}
