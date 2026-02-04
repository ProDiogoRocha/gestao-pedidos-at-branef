
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Orders.Application.Orders.Models;

namespace Orders.Infrastructure.Persistence.Mongo;

public sealed class MongoContext
{
    public IMongoDatabase Database { get; }
    public IMongoCollection<OrderReadDto> Orders { get; }
    public IMongoCollection<ProductDto> Products { get; }

    public MongoContext(IMongoClient client, IOptions<MongoSettings> options)
    {
        Database = client.GetDatabase(options.Value.Database);

        Orders = Database.GetCollection<OrderReadDto>("orders_read");
        Products = Database.GetCollection<ProductDto>("products_read");

        Orders.Indexes.CreateOne(new CreateIndexModel<OrderReadDto>(
            Builders<OrderReadDto>.IndexKeys.Descending(x => x.OrderDate)));
    }
}
