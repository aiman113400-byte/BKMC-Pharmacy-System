using BKMC.Pharmacy.Web.Models.Patients;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Patients;

public class IndexModel : PageModel
{
    private readonly PatientApiService _patientApiService;

    public IndexModel(
        PatientApiService patientApiService)
    {
        _patientApiService = patientApiService;
    }

    public List<PatientDto> Patients { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Patients =
                await _patientApiService.GetAllAsync();

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Unable to load patients. {ex.Message}";

            return Page();
        }
    }
}