namespace VeniceOrders.Application.DTOs;

/// <summary>
/// DTO de resposta agregando dados do SQL Server e MongoDB
/// Conforme requisito: "retornar o pedido com os dados vindos dos dois bancos integrados"
/// </summary>
public class OrderResponseDto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public DateTime Data { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<OrderItemResponseDto> Itens { get; set; } = new();
}

public class OrderItemResponseDto
{
    public Guid Id { get; set; }
    public string Produto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
