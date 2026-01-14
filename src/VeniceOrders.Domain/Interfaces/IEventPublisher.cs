namespace VeniceOrders.Domain.Interfaces;

/// <summary>
/// Interface para publicação de eventos na fila de mensageria
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
}
