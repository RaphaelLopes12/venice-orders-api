using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VeniceOrders.Infrastructure.Persistence.MongoDB;

public class OrderItemDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid OrderId { get; set; }

    public string Produto { get; set; } = string.Empty;

    public int Quantidade { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal PrecoUnitario { get; set; }
}
