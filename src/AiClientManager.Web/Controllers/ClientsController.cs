using AiClientManager.Web.Models;
using AiClientManager.Web.Services;
using AiClientManager.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiClientManager.Web.Controllers;

[Authorize]
public class ClientsController : Controller
{
    private readonly ClientRepository _repo;
    private readonly CvFileService _files;
    private readonly AiAnalysisService _ai;
    private readonly OpenAiSettings _aiSettings;

    public ClientsController(ClientRepository repo, CvFileService files, AiAnalysisService ai, Microsoft.Extensions.Options.IOptions<OpenAiSettings> aiSettings)
    {
        _repo = repo;
        _files = files;
        _ai = ai;
        _aiSettings = aiSettings.Value;
    }

    public async Task<IActionResult> Index(string? q, ClientPriority? priority, CancellationToken ct)
    {
        ViewBag.Query = q;
        ViewBag.Priority = priority;
        var clients = await _repo.SearchAsync(q, priority, ct);
        return View(clients);
    }

    public async Task<IActionResult> Details(string id, CancellationToken ct)
    {
        var client = await _repo.GetByIdAsync(id, ct);
        if (client is null) return NotFound();
        return View(client);
    }

    public IActionResult Create() => View(new ClientEditVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var cv = await _files.SaveCvAsync(vm.CvFile, ct);

        var doc = new ClientDocument
        {
            Name = vm.Name,
            Email = vm.Email,
            Phone = vm.Phone,
            Company = vm.Company,
            Notes = vm.Notes,
            Cv = cv
        };

        if (!string.IsNullOrWhiteSpace(vm.NewInteraction))
        {
            doc.Interactions.Add(new InteractionNote { DateUtc = DateTime.UtcNow, Text = vm.NewInteraction.Trim() });
        }

        if (vm.AnalyzeNow || _aiSettings.AutoAnalyzeOnSave)
        {
            doc.Analysis = await _ai.AnalyzeAsync(doc, ct);
        }

        await _repo.CreateAsync(doc, ct);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id, CancellationToken ct)
    {
        var client = await _repo.GetByIdAsync(id, ct);
        if (client is null) return NotFound();

        var vm = new ClientEditVm
        {
            Id = client.Id,
            Name = client.Name,
            Email = client.Email,
            Phone = client.Phone,
            Company = client.Company,
            Notes = client.Notes
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ClientEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);
        if (string.IsNullOrWhiteSpace(vm.Id)) return BadRequest();

        var client = await _repo.GetByIdAsync(vm.Id, ct);
        if (client is null) return NotFound();

        client.Name = vm.Name;
        client.Email = vm.Email;
        client.Phone = vm.Phone;
        client.Company = vm.Company;
        client.Notes = vm.Notes;

        if (!string.IsNullOrWhiteSpace(vm.NewInteraction))
        {
            client.Interactions.Add(new InteractionNote { DateUtc = DateTime.UtcNow, Text = vm.NewInteraction.Trim() });
        }

        if (vm.CvFile is not null && vm.CvFile.Length > 0)
        {
            client.Cv = await _files.SaveCvAsync(vm.CvFile, ct);
        }

        if (vm.AnalyzeNow || _aiSettings.AutoAnalyzeOnSave)
        {
            client.Analysis = await _ai.AnalyzeAsync(client, ct);
        }

        await _repo.UpdateAsync(client, ct);
        return RedirectToAction(nameof(Details), new { id = client.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _repo.DeleteAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Analyze(string id, CancellationToken ct)
    {
        var client = await _repo.GetByIdAsync(id, ct);
        if (client is null) return NotFound();

        client.Analysis = await _ai.AnalyzeAsync(client, ct);
        await _repo.UpdateAsync(client, ct);

        return RedirectToAction(nameof(Details), new { id });
    }
}
