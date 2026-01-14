namespace VeniceOrders.Domain.Entities;

/// <summary>
/// Item do pedido - armazenado no MongoDB
/// Conforme requisito: produto, quantidade e preço unitário
/// </summary>
public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string Produto { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }

    protected OrderItem() { }

    public OrderItem(Guid orderId, string produto, int quantidade, decimal precoUnitario)
    {
        if (string.IsNullOrWhiteSpace(produto))
            throw new ArgumentException("Produto não pode ser vazio", nameof(produto));

        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantidade));

        if (precoUnitario < 0)
            throw new ArgumentException("Preço unitário não pode ser negativo", nameof(precoUnitario));

        Id = Guid.NewGuid();
        OrderId = orderId;
        Produto = produto;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
    }

    /// <summary>
    /// Reconstrói entidade a partir de dados persistidos
    /// </summary>
    public static OrderItem Reconstruct(Guid id, Guid orderId, string produto, int quantidade, decimal precoUnitario)
    {
        var item = new OrderItem
        {
            Id = id,
            OrderId = orderId,
            Produto = produto,
            Quantidade = quantidade,
            PrecoUnitario = precoUnitario
        };
        return item;
    }

    public decimal Subtotal => Quantidade * PrecoUnitario;
}
