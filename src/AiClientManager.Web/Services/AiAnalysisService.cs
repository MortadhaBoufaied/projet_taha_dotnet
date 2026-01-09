using AiClientManager.Web.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AiClientManager.Web.Services;

public sealed class AiAnalysisService
{
    private readonly OpenAiSettings _settings;
    private readonly HttpClient _http;

    public AiAnalysisService(IOptions<OpenAiSettings> options)
    {
        _settings = options.Value;
        _http = new HttpClient();
    }

    public async Task<ClientAnalysis> AnalyzeAsync(ClientDocument client, CancellationToken ct)
    {
        if (_settings.Enabled && !string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            try
            {
                return await AnalyzeWithOpenAiAsync(client, ct);
            }
            catch
            {
                // fallback
            }
        }

        return AnalyzeHeuristically(client);
    }

    private async Task<ClientAnalysis> AnalyzeWithOpenAiAsync(ClientDocument client, CancellationToken ct)
    {
        var baseUrl = _settings.BaseUrl.TrimEnd('/');
        var url = $"{baseUrl}/v1/chat/completions";

        var notes = BuildAllNotes(client);
        var prompt = $@"Tu es un assistant CRM.
À partir des notes/CV ci-dessous, produis UNIQUEMENT un JSON valide avec exactement ces champs:
{{ ""priority"": ""High|Medium|Low"", ""summary"": ""string"", ""keywords"": [""string""] }}
Règles:
- priority = High si client très prometteur/urgent; Medium sinon; Low si faible potentiel.
- keywords: 5 à 12 mots-clés courts, sans doublons.

DONNÉES CLIENT:
Nom: {client.Name}
Société: {client.Company}
Email: {client.Email}
Téléphone: {client.Phone}

NOTES/CV:
{notes}";


        var body = new
        {
            model = _settings.Model,
            messages = new object[]
            {
                new { role = "system", content = "You are a helpful assistant that outputs strict JSON." },
                new { role = "user", content = prompt }
            },
            temperature = 0.2
        };

        var json = JsonSerializer.Serialize(body);
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var resp = await _http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();
        var payload = await resp.Content.ReadAsStringAsync(ct);

        using var doc = JsonDocument.Parse(payload);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
            return AnalyzeHeuristically(client);

        // Remove Markdown fences if any
        content = content.Trim();
        if (content.StartsWith("```"))
        {
            var idx = content.IndexOf("```", 3, StringComparison.Ordinal);
            if (idx > 0)
                content = content.Substring(3, idx - 3).Trim();
        }

        var parsed = JsonSerializer.Deserialize<OpenAiOutput>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (parsed is null)
            return AnalyzeHeuristically(client);

        return new ClientAnalysis
        {
            Priority = parsed.Priority.ToPriority(),
            Summary = parsed.Summary ?? string.Empty,
            Keywords = parsed.Keywords?
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(12)
                .ToList() ?? new List<string>(),
            GeneratedAtUtc = DateTime.UtcNow,
            Source = "openai"
        };
    }

    private static string BuildAllNotes(ClientDocument client)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(client.Notes))
            sb.AppendLine(client.Notes);

        if (client.Interactions != null && client.Interactions.Count > 0)
        {
            sb.AppendLine("--- Interactions ---");
            foreach (var it in client.Interactions.OrderByDescending(i => i.DateUtc).Take(20))
                sb.AppendLine($"[{it.DateUtc:u}] {it.Text}");
        }

        if (!string.IsNullOrWhiteSpace(client.Cv?.ExtractedText))
        {
            sb.AppendLine("--- CV Extract ---");
            sb.AppendLine(client.Cv.ExtractedText);
        }

        return sb.ToString();
    }

    private static ClientAnalysis AnalyzeHeuristically(ClientDocument client)
    {
        var text = (client.Notes ?? "") + "\n" +
                   string.Join("\n", client.Interactions?.Select(i => i.Text) ?? Array.Empty<string>()) +
                   "\n" + (client.Cv?.ExtractedText ?? "");
        var lowered = text.ToLowerInvariant();

        var highSignals = new[] { "urgent", "asap", "immédiat", "budget", "contrat", "vip", "important", "meeting", "rendez" };
        var lowSignals = new[] { "pas intéress", "refus", "abandon", "no need", "sans suite" };

        var score = 0;
        score += highSignals.Count(s => lowered.Contains(s)) * 2;
        score -= lowSignals.Count(s => lowered.Contains(s)) * 2;

        var priority = score >= 2 ? ClientPriority.High : score <= -1 ? ClientPriority.Low : ClientPriority.Medium;

        var keywords = ExtractKeywords(text, 10);
        var summary = string.IsNullOrWhiteSpace(text)
            ? "Aucune note fournie."
            : (text.Length <= 240 ? text : text[..240] + "...");

        return new ClientAnalysis
        {
            Priority = priority,
            Summary = summary,
            Keywords = keywords,
            GeneratedAtUtc = DateTime.UtcNow,
            Source = "heuristic"
        };
    }

    private static List<string> ExtractKeywords(string text, int take)
    {
        var stop = new HashSet<string>(new[]
        {
            "the","and","for","with","this","that","from","have","has","are","was","were",
            "les","des","une","un","et","pour","avec","dans","sur","par","est","sont","être","avoir",
            "de","la","le","du","au","aux","en","à","a","d"
        }, StringComparer.OrdinalIgnoreCase);

        var words = text
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(w => new string(w.Where(char.IsLetterOrDigit).ToArray()))
            .Where(w => w.Length >= 4)
            .Where(w => !stop.Contains(w));

        return words
            .GroupBy(w => w, StringComparer.OrdinalIgnoreCase)
            .Select(g => (Word: g.Key, Count: g.Count()))
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Word)
            .Take(take)
            .Select(x => x.Word)
            .ToList();
    }

    private sealed class OpenAiOutput
    {
        public string? Priority { get; set; }
        public string? Summary { get; set; }
        public List<string>? Keywords { get; set; }
    }
}

internal static class PriorityMap
{
    public static ClientPriority ToPriority(this string? p)
    {
        return (p ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "high" => ClientPriority.High,
            "medium" => ClientPriority.Medium,
            "low" => ClientPriority.Low,
            _ => ClientPriority.Unknown
        };
    }
}
