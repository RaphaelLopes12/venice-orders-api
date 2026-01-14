namespace VeniceOrders.Domain.Entities;

/// <summary>
/// Entidade principal do pedido - armazenada no SQL Server
/// </summary>
public class Order
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public DateTime Data { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Total { get; private set; }

    protected Order() { }

    public Order(Guid clienteId)
    {
        Id = Guid.NewGuid();
        ClienteId = clienteId;
        Data = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        Total = 0;
    }

    public void SetTotal(decimal total)
    {
        if (total < 0)
            throw new ArgumentException("Total não pode ser negativo", nameof(total));

        Total = total;
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
    }
}
