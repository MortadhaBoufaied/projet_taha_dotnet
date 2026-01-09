using AiClientManager.Web.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace AiClientManager.Web.Services;

public sealed class MongoContext
{
    public IMongoDatabase Database { get; }
    public IMongoCollection<ClientDocument> Clients { get; }
    public GridFSBucket CvBucket { get; }

    public MongoContext(IOptions<MongoSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        Database = client.GetDatabase(settings.DatabaseName);
        Clients = Database.GetCollection<ClientDocument>(settings.ClientsCollectionName);
        CvBucket = new GridFSBucket(Database, new GridFSBucketOptions { BucketName = settings.GridFsBucketName });

        // Ensure useful indexes
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        var idxKeys = Builders<ClientDocument>.IndexKeys
            .Text(x => x.Name)
            .Text(x => x.Company)
            .Text(x => x.Notes);

        var textIndex = new CreateIndexModel<ClientDocument>(idxKeys, new CreateIndexOptions { Name = "clients_text" });
        Clients.Indexes.CreateOne(textIndex);

        var updatedIndex = new CreateIndexModel<ClientDocument>(
            Builders<ClientDocument>.IndexKeys.Descending(x => x.UpdatedAtUtc),
            new CreateIndexOptions { Name = "updated_desc" });
        Clients.Indexes.CreateOne(updatedIndex);
    }

    public async Task<bool> PingAsync(CancellationToken ct)
    {
        try
        {
            var cmd = new BsonDocument("ping", 1);
            await Database.RunCommandAsync<BsonDocument>(cmd, cancellationToken: ct);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
