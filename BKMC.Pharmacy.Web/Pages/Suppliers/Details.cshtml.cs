using BKMC.Pharmacy.Web.Models.Suppliers;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http;

namespace BKMC.Pharmacy.Web.Pages.Suppliers;

public class DetailsModel : PageModel
{
    private readonly SupplierApiService _supplierApiService;
    private readonly ILogger<DetailsModel> _logger;


public DetailsModel(
    SupplierApiService supplierApiService,
    ILogger<DetailsModel> logger)
    {
        _supplierApiService = supplierApiService;
        _logger = logger;
    }

    public SupplierDto Supplier { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var supplier = await _supplierApiService.GetByIdAsync(id);

            if (supplier == null)
                return NotFound();

            Supplier = supplier;

            return Page();
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == HttpStatusCode.Unauthorized ||
                  ex.StatusCode == HttpStatusCode.Forbidden)
        {
            return RedirectToPage("/Suppliers/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load supplier details for ID {SupplierId}.",
                id);

            ErrorMessage =
                "Unable to load supplier details. Please check the API connection.";

            return Page();
        }
    }


}
