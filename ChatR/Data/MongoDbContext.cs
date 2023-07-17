using MongoDB.Driver;

namespace ChatR.Data;

public class MongoDbContext : MongoClient
{
    public MongoDbContext(IConfiguration configuration)
        : base(configuration.GetConnectionString("MongoDbConnection"))
    {
    }
}