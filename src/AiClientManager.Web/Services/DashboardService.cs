using AiClientManager.Web.ViewModels;
using MongoDB.Driver;

namespace AiClientManager.Web.Services;

public sealed class DashboardService
{
    private readonly MongoContext _mongo;

    public DashboardService(MongoContext mongo)
    {
        _mongo = mongo;
    }

    public async Task<DashboardVm> GetAsync(CancellationToken ct)
    {
        var clients = await _mongo.Clients.Find(_ => true).ToListAsync(ct);

        var total = clients.Count;
        var high = clients.Count(c => c.Analysis?.Priority == Models.ClientPriority.High);
        var med = clients.Count(c => c.Analysis?.Priority == Models.ClientPriority.Medium);
        var low = clients.Count(c => c.Analysis?.Priority == Models.ClientPriority.Low);
        var unanalyzed = clients.Count(c => c.Analysis is null || c.Analysis.Priority == Models.ClientPriority.Unknown);

        var byCompany = clients
            .Where(c => !string.IsNullOrWhiteSpace(c.Company))
            .GroupBy(c => c.Company!.Trim())
            .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
            .OrderByDescending(kv => kv.Value)
            .Take(8)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        var byMonth = clients
            .GroupBy(c => new DateTime(c.CreatedAtUtc.Year, c.CreatedAtUtc.Month, 1))
            .OrderBy(g => g.Key)
            .TakeLast(12)
            .ToDictionary(g => g.Key.ToString("yyyy-MM"), g => g.Count());

        return new DashboardVm
        {
            TotalClients = total,
            High = high,
            Medium = med,
            Low = low,
            Unanalyzed = unanalyzed,
            ClientsByCompany = byCompany,
            ClientsByMonth = byMonth
        };
    }
}
