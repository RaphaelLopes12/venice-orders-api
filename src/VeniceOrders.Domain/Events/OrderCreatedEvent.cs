namespace VeniceOrders.Domain.Events;

/// <summary>
/// Evento PedidoCriado - publicado na fila após criação do pedido
/// Será consumido pelo sistema de faturamento
/// </summary>
public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public Guid ClienteId { get; set; }
    public DateTime Data { get; set; }
    public decimal Total { get; set; }
    public int QuantidadeItens { get; set; }
    public DateTime CreatedAt { get; set; }

    public OrderCreatedEvent()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public OrderCreatedEvent(Guid orderId, Guid clienteId, DateTime data, decimal total, int quantidadeItens)
        : this()
    {
        OrderId = orderId;
        ClienteId = clienteId;
        Data = data;
        Total = total;
        QuantidadeItens = quantidadeItens;
    }
}
