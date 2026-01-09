using AiClientManager.Web.Models;
using MongoDB.Driver;

namespace AiClientManager.Web.Services;

public sealed class ClientRepository
{
    private readonly MongoContext _mongo;

    public ClientRepository(MongoContext mongo)
    {
        _mongo = mongo;
    }

    public async Task<List<ClientDocument>> GetAllAsync(CancellationToken ct)
    {
        return await _mongo.Clients.Find(FilterDefinition<ClientDocument>.Empty)
            .SortByDescending(c => c.UpdatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<ClientDocument?> GetByIdAsync(string id, CancellationToken ct)
    {
        return await _mongo.Clients.Find(c => c.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<ClientDocument> CreateAsync(ClientDocument client, CancellationToken ct)
    {
        client.CreatedAtUtc = DateTime.UtcNow;
        client.UpdatedAtUtc = DateTime.UtcNow;
        await _mongo.Clients.InsertOneAsync(client, cancellationToken: ct);
        return client;
    }

    public async Task<bool> UpdateAsync(ClientDocument client, CancellationToken ct)
    {
        client.UpdatedAtUtc = DateTime.UtcNow;
        var result = await _mongo.Clients.ReplaceOneAsync(c => c.Id == client.Id, client, cancellationToken: ct);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct)
    {
        var existing = await GetByIdAsync(id, ct);
        if (existing?.Cv?.FileId is { Length: > 0 })
        {
            // Try delete file from GridFS (ignore failures)
            try
            {
                var oid = MongoDB.Bson.ObjectId.Parse(existing.Cv.FileId);
                await _mongo.CvBucket.DeleteAsync(oid, ct);
            }
            catch { }
        }

        var result = await _mongo.Clients.DeleteOneAsync(c => c.Id == id, ct);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    public async Task<List<ClientDocument>> SearchAsync(string? query, ClientPriority? priority, CancellationToken ct)
    {
        FilterDefinition<ClientDocument> filter = FilterDefinition<ClientDocument>.Empty;

        if (!string.IsNullOrWhiteSpace(query))
        {
            // Use $text when possible
            var text = Builders<ClientDocument>.Filter.Text(query);
            var regex = Builders<ClientDocument>.Filter.Or(
                Builders<ClientDocument>.Filter.Regex(c => c.Name, new MongoDB.Bson.BsonRegularExpression(query, "i")),
                Builders<ClientDocument>.Filter.Regex(c => c.Company, new MongoDB.Bson.BsonRegularExpression(query, "i")),
                Builders<ClientDocument>.Filter.Regex(c => c.Notes, new MongoDB.Bson.BsonRegularExpression(query, "i"))
            );
            filter = Builders<ClientDocument>.Filter.Or(text, regex);
        }

        if (priority is not null && priority != ClientPriority.Unknown)
        {
            var p = priority.Value;
            var pf = Builders<ClientDocument>.Filter.Eq(c => c.Analysis!.Priority, p);
            // include clients without analysis if priority filter is Unknown only
            filter = Builders<ClientDocument>.Filter.And(filter, pf);
        }

        return await _mongo.Clients.Find(filter)
            .SortByDescending(c => c.UpdatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task AddInteractionAsync(string id, string note, CancellationToken ct)
    {
        var update = Builders<ClientDocument>.Update
            .Push(c => c.Interactions, new InteractionNote { DateUtc = DateTime.UtcNow, Text = note })
            .Set(c => c.UpdatedAtUtc, DateTime.UtcNow);

        await _mongo.Clients.UpdateOneAsync(c => c.Id == id, update, cancellationToken: ct);
    }
}
