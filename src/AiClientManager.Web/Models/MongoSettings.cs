using System.ComponentModel.DataAnnotations;

namespace AiClientManager.Web.Models;

public sealed class MongoSettings
{
    [Required]
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";

    [Required]
    public string DatabaseName { get; set; } = "AiClientManagerDb";

    public string ClientsCollectionName { get; set; } = "clients";

    public string GridFsBucketName { get; set; } = "cvFiles";
}
