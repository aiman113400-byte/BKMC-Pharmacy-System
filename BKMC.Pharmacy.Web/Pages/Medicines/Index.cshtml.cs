using BKMC.Pharmacy.Web.Models.Medicines;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Medicines;

public class IndexModel : PageModel
{
    private readonly MedicineApiService _medicineApiService;

    public IndexModel(MedicineApiService medicineApiService)
    {
        _medicineApiService = medicineApiService;
    }

    public List<MedicineDto> Medicines { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadMedicinesAsync();
    }

    public async Task<IActionResult> OnPostActivateAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var success =
                await _medicineApiService.ActivateAsync(id);

            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Medicine was not found.";

                return RedirectToPage();
            }

            TempData["SuccessMessage"] =
                "Medicine activated successfully.";

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] =
                $"Unable to activate medicine. {ex.Message}";

            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var success =
                await _medicineApiService.DeactivateAsync(id);

            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Medicine was not found.";

                return RedirectToPage();
            }

            TempData["SuccessMessage"] =
                "Medicine deactivated successfully.";

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] =
                $"Unable to deactivate medicine. {ex.Message}";

            return RedirectToPage();
        }
    }

    private async Task LoadMedicinesAsync()
    {
        try
        {
            Medicines =
                await _medicineApiService.GetAllAsync(
                    Search,
                    IsActive);
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Unable to load medicines. {ex.Message}";

            Medicines = new List<MedicineDto>();
        }
    }
}