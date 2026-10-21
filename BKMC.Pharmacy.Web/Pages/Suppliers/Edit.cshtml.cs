using BKMC.Pharmacy.Web.Models.Suppliers;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http;

namespace BKMC.Pharmacy.Web.Pages.Suppliers;

public class EditModel : PageModel
{
    private readonly SupplierApiService _supplierApiService;
    private readonly ILogger<EditModel> _logger;


public EditModel(
    SupplierApiService supplierApiService,
    ILogger<EditModel> logger)
    {
        _supplierApiService = supplierApiService;
        _logger = logger;
    }

    [BindProperty]
    public UpdateSupplierModel Supplier { get; set; } = new();

    public int SupplierId { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var supplier = await _supplierApiService.GetByIdAsync(id);

            if (supplier == null)
                return NotFound();

            SupplierId = supplier.Id;

            Supplier = new UpdateSupplierModel
            {
                SupplierCode = supplier.SupplierCode,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Phone = supplier.Phone,
                Email = supplier.Email,
                Address = supplier.Address,
                IsActive = supplier.IsActive
            };

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
            _logger.LogError(ex, "Failed to load supplier {SupplierId}.", id);

            return StatusCode(500, "Unable to load supplier details.");
        }
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        SupplierId = id;

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var updatedSupplier =
                await _supplierApiService.UpdateAsync(id, Supplier);

            if (updatedSupplier == null)
                return NotFound();

            TempData["Message"] = "Supplier updated successfully.";

            return RedirectToPage("/Suppliers/Index");
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            ModelState.AddModelError(
                "Supplier.SupplierCode",
                "This supplier code is already used by another supplier.");

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
            _logger.LogError(ex, "Failed to update supplier {SupplierId}.", id);

            ErrorMessage =
                "Unable to update supplier. Please check the API connection and try again.";

            return Page();
        }
    }


}
