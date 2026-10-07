using BKMC.Pharmacy.Web.Models.Patients;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Patients;

public class CreateModel : PageModel
{
    private readonly PatientApiService _patientApiService;

    public CreateModel(
        PatientApiService patientApiService)
    {
        _patientApiService = patientApiService;
    }

    [BindProperty]
    public CreatePatientModel Patient { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var result =
                await _patientApiService.CreateAsync(Patient);

            if (result == null)
            {
                ErrorMessage =
                    "Unable to create patient. Please try again.";

                return Page();
            }

            return RedirectToPage("/Patients/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Unable to create patient. {ex.Message}";

            return Page();
        }
    }
}