using AiClientManager.Web.Models;
using AiClientManager.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiClientManager.Web.Controllers.Api;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsApiController : ControllerBase
{
    private readonly ClientRepository _repo;
    private readonly AiAnalysisService _ai;

    public ClientsApiController(ClientRepository repo, AiAnalysisService ai)
    {
        _repo = repo;
        _ai = ai;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClientDocument>>> GetAll([FromQuery] string? q, [FromQuery] ClientPriority? priority, CancellationToken ct)
    {
        var list = await _repo.SearchAsync(q, priority, ct);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDocument>> GetById(string id, CancellationToken ct)
    {
        var client = await _repo.GetByIdAsync(id, ct);
        return client is null ? NotFound() : Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult<ClientDocument>> Create([FromBody] ClientDocument client, CancellationToken ct)
    {
        client.Id = null;
        var created = await _repo.CreateAsync(client, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] ClientDocument client, CancellationToken ct)
    {
        client.Id = id;
        var ok = await _repo.UpdateAsync(client, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var ok = await _repo.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id}/analyze")]
    public async Task<ActionResult<ClientAnalysis>> Analyze(string id, CancellationToken ct)
    {
        var client = await _repo.GetByIdAsync(id, ct);
        if (client is null) return NotFound();

        client.Analysis = await _ai.AnalyzeAsync(client, ct);
        await _repo.UpdateAsync(client, ct);
        return Ok(client.Analysis);
    }
}
