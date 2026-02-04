
namespace Orders.Infrastructure.Persistence.Mongo;

public sealed class MongoSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "gestao_pedidos";
}
