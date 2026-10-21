
using BKMC.Pharmacy.Web.Models.Suppliers;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Suppliers;

public class IndexModel : PageModel
{
    private readonly SupplierApiService _supplierApiService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        SupplierApiService supplierApiService,
        ILogger<IndexModel> logger)
    {
        _supplierApiService = supplierApiService;
        _logger = logger;
    }

    public List<SupplierDto> Suppliers { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadSuppliersAsync();
    }

    private async Task LoadSuppliersAsync()
    {
        try
        {
            Suppliers = string.IsNullOrWhiteSpace(Search)
                ? await _supplierApiService.GetAllAsync()
                : await _supplierApiService.SearchAsync(Search);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load suppliers.");
            ErrorMessage =
                "Unable to load suppliers. Please check your login and API connection.";
        }
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        try
        {
            var result = await _supplierApiService.DeactivateAsync(id);

            TempData["Message"] = result
                ? "Supplier deactivated successfully."
                : "Supplier not found.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate supplier {Id}.", id);
            TempData["Error"] =
                "Unable to deactivate supplier. Please check your access and API connection.";
        }

        return RedirectToPage(new { Search });
    }

    public async Task<IActionResult> OnPostActivateAsync(int id)
    {
        try
        {
            var result = await _supplierApiService.ActivateAsync(id);

            TempData["Message"] = result
                ? "Supplier activated successfully."
                : "Supplier not found.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate supplier {Id}.", id);
            TempData["Error"] =
                "Unable to activate supplier. Please check your access and API connection.";
        }

        return RedirectToPage(new { Search });
    }
}