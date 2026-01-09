using System.ComponentModel.DataAnnotations;

namespace AiClientManager.Web.ViewModels;

public sealed class ClientEditVm
{
    public string? Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Company { get; set; }

    [Display(Name = "Notes / Description")]
    public string? Notes { get; set; }

    [Display(Name = "Ajouter une note (interaction)")]
    public string? NewInteraction { get; set; }

    public IFormFile? CvFile { get; set; }

    public bool AnalyzeNow { get; set; }
}
