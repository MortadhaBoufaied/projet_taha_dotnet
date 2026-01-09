namespace AiClientManager.Web.Models;

public sealed class ClientAnalysis
{
    public ClientPriority Priority { get; set; } = ClientPriority.Unknown;
    public string Summary { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "heuristic"; // openai|heuristic
}
