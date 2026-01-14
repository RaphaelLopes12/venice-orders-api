using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VeniceOrders.Application.Interfaces;
using VeniceOrders.Domain.Interfaces;
using VeniceOrders.Infrastructure.Caching;
using VeniceOrders.Infrastructure.Messaging;
using VeniceOrders.Infrastructure.Persistence.MongoDB;
using VeniceOrders.Infrastructure.Persistence.SqlServer;

namespace VeniceOrders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServer")));

        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDb"));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "VeniceOrders:";
        });

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("RabbitMq"));
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();

        services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}
