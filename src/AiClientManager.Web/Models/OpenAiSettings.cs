namespace AiClientManager.Web.Models;

public sealed class OpenAiSettings
{
    public bool Enabled { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openai.com";
    public string Model { get; set; } = "gpt-4o-mini";
    public bool AutoAnalyzeOnSave { get; set; }
}
