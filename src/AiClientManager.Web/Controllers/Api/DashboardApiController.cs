using AiClientManager.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiClientManager.Web.Controllers.Api;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardApiController : ControllerBase
{
    private readonly DashboardService _dash;

    public DashboardApiController(DashboardService dash)
    {
        _dash = dash;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
    {
        var vm = await _dash.GetAsync(ct);
        return Ok(vm);
    }
}
