
using BKMC.Pharmacy.Web.Models.Suppliers;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http;

namespace BKMC.Pharmacy.Web.Pages.Suppliers;

public class CreateModel : PageModel
{
    private readonly SupplierApiService _supplierApiService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        SupplierApiService supplierApiService,
        ILogger<CreateModel> logger)
    {
        _supplierApiService = supplierApiService;
        _logger = logger;
    }

    [BindProperty]
    public CreateSupplierModel Supplier { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _supplierApiService.CreateAsync(Supplier);

            TempData["Message"] = "Supplier created successfully.";

            return RedirectToPage("/Suppliers/Index");
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            ModelState.AddModelError(
                "Supplier.SupplierCode",
                "This supplier code already exists.");

            return Page();
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == HttpStatusCode.Unauthorized ||
                  ex.StatusCode == HttpStatusCode.Forbidden)
        {
            ErrorMessage =
                "Access denied. Please sign in with an authorized Admin account.";

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create supplier.");

            ErrorMessage =
                "Unable to create supplier. Please check the API connection and try again.";

            return Page();
        }
    }
}