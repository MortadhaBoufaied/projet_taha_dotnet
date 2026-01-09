using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AiClientManager.Web.Models;

public sealed class ClientDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }

    /// <summary>Main free text notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Unlimited notes/history entries (recommended by your prof).</summary>
    public List<InteractionNote> Interactions { get; set; } = new();

    public CvFile? Cv { get; set; }
    public ClientAnalysis? Analysis { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
