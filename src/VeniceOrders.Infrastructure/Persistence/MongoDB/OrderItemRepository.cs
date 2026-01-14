using Microsoft.Extensions.Options;
using MongoDB.Driver;
using VeniceOrders.Domain.Entities;
using VeniceOrders.Domain.Interfaces;

namespace VeniceOrders.Infrastructure.Persistence.MongoDB;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly IMongoCollection<OrderItemDocument> _collection;

    public OrderItemRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<OrderItemDocument>(settings.Value.OrderItemsCollectionName);
    }

    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var documents = await _collection
            .Find(x => x.OrderId == orderId)
            .ToListAsync(cancellationToken);

        return documents.Select(MapToEntity);
    }

    public async Task AddManyAsync(IEnumerable<OrderItem> items, CancellationToken cancellationToken = default)
    {
        var documents = items.Select(MapToDocument).ToList();

        if (documents.Count != 0)
        {
            await _collection.InsertManyAsync(documents, cancellationToken: cancellationToken);
        }
    }

    private static OrderItemDocument MapToDocument(OrderItem item)
    {
        return new OrderItemDocument
        {
            Id = item.Id,
            OrderId = item.OrderId,
            Produto = item.Produto,
            Quantidade = item.Quantidade,
            PrecoUnitario = item.PrecoUnitario
        };
    }

    private static OrderItem MapToEntity(OrderItemDocument document)
    {
        return OrderItem.Reconstruct(
            document.Id,
            document.OrderId,
            document.Produto,
            document.Quantidade,
            document.PrecoUnitario
        );
    }
}
