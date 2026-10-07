using BKMC.Pharmacy.Web.Models.Patients;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Patients;

public class EditModel : PageModel
{
    private readonly PatientApiService _patientApiService;

    public EditModel(PatientApiService patientApiService)
    {
        _patientApiService = patientApiService;
    }

    [BindProperty]
    public UpdatePatientModel Patient { get; set; } = new();

    public int PatientId { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var patient = await _patientApiService.GetByIdAsync(id);

            if (patient == null)
            {
                return NotFound();
            }

            PatientId = patient.Id;

            Patient = new UpdatePatientModel
            {
                MRNumber = patient.MRNumber,
                Name = patient.Name,
                Age = patient.Age,
                Gender = patient.Gender,
                AdmissionNumber = patient.AdmissionNumber,
                ReferralId = patient.ReferralId
            };

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Unable to load patient. {ex.Message}";

            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            PatientId = id;
            return Page();
        }

        try
        {
            var result =
                await _patientApiService.UpdateAsync(id, Patient);

            if (result == null)
            {
                PatientId = id;

                ErrorMessage =
                    "Unable to update patient. Please try again.";

                return Page();
            }

            return RedirectToPage("/Patients/Index");
        }
        catch (Exception ex)
        {
            PatientId = id;

            ErrorMessage =
                $"Unable to update patient. {ex.Message}";

            return Page();
        }
    }
}