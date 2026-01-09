using AiClientManager.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiClientManager.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly DashboardService _dash;

    public HomeController(DashboardService dash)
    {
        _dash = dash;
    }

    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var vm = await _dash.GetAsync(ct);
        return View(vm);
    }

    [AllowAnonymous]
    public IActionResult Error() => View();
}
