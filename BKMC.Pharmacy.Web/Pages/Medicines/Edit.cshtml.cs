using BKMC.Pharmacy.Web.Models.Medicines;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Medicines;

public class EditModel : PageModel
{
    private readonly MedicineApiService _medicineApiService;

    public EditModel(MedicineApiService medicineApiService)
    {
        _medicineApiService = medicineApiService;
    }

    [BindProperty]
    public UpdateMedicineModel Medicine { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (Id <= 0)
        {
            return NotFound();
        }

        try
        {
            var medicine = await _medicineApiService.GetByIdAsync(Id);

            if (medicine == null)
            {
                return NotFound();
            }

         
            Medicine.Name = medicine.Name;
            Medicine.GenericName = medicine.GenericName;
            Medicine.Strength = medicine.Strength;
            Medicine.Form = medicine.Form;

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Unable to load medicine. {ex.Message}";

            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Id <= 0)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var updatedMedicine =
                await _medicineApiService.UpdateAsync(
                    Id,
                    Medicine);

            if (updatedMedicine == null)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "Medicine updated successfully.";

            return RedirectToPage("/Medicines/Index");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Unable to update medicine. {ex.Message}";

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"An unexpected error occurred. {ex.Message}";

            return Page();
        }
    }
}