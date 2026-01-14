namespace VeniceOrders.Application.Interfaces;

/// <summary>
/// Interface para serviço de cache
/// Requisito: "Incluir cache Redis para GET - Cachear a resposta de pedidos por 2 minutos"
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
