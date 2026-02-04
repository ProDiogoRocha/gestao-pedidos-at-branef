
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Orders.Application.Common.Interfaces;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Persistence.Mongo;
using Orders.Infrastructure.Repositories;
using Orders.Infrastructure.Services;

namespace Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrdersDbContext>(opt =>
        {
            opt.UseSqlServer(configuration.GetConnectionString("SqlServer"));
        });

        // Mongo
        services.Configure<MongoSettings>(configuration.GetSection("Mongo"));
        services.AddSingleton<IMongoClient>(_ =>
        {
            var settings = configuration.GetSection("Mongo").Get<MongoSettings>() ?? new MongoSettings();
            return new MongoClient(settings.ConnectionString);
        });

        services.AddSingleton<MongoContext>();

        services.AddScoped<IOrderWriteRepository, OrderWriteRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<IOrderReadRepository, OrderReadRepository>();
        services.AddScoped<IOrderProjectionWriter, OrderProjectionWriter>();

        services.AddSingleton<IFixedCustomerProvider, FixedCustomerProvider>();

        services.AddHostedService<OutboxProcessorHostedService>();
        services.AddHostedService<DatabaseInitializerHostedService>();

        return services;
    }
}
