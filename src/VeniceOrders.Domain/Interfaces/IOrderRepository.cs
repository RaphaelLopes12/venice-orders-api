using VeniceOrders.Domain.Entities;

namespace VeniceOrders.Domain.Interfaces;

/// <summary>
/// Repositório para Order - implementado com SQL Server
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
}
