namespace AiClientManager.Web.ViewModels;

public sealed class DashboardVm
{
    public int TotalClients { get; set; }
    public int High { get; set; }
    public int Medium { get; set; }
    public int Low { get; set; }
    public int Unanalyzed { get; set; }

    public Dictionary<string, int> ClientsByCompany { get; set; } = new();
    public Dictionary<string, int> ClientsByMonth { get; set; } = new();
}
