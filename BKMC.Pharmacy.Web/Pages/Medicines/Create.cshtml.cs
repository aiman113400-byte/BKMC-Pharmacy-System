using BKMC.Pharmacy.Web.Models.Medicines;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Medicines;

public class CreateModel : PageModel
{
    private readonly MedicineApiService _medicineApiService;

    public CreateModel(MedicineApiService medicineApiService)
    {
        _medicineApiService = medicineApiService;
    }

    [BindProperty]
    public CreateMedicineModel Medicine { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _medicineApiService.CreateAsync(Medicine);

            TempData["SuccessMessage"] =
                "Medicine created successfully.";

            return RedirectToPage("/Medicines/Index");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Unable to create medicine. {ex.Message}";

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