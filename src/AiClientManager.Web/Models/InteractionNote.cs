namespace AiClientManager.Web.Models;

public sealed class InteractionNote
{
    public DateTime DateUtc { get; set; } = DateTime.UtcNow;
    public string Text { get; set; } = string.Empty;
}
