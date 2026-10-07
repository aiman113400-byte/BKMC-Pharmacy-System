using BKMC.Pharmacy.Web.Models.Medicines;
using BKMC.Pharmacy.Web.Models.Receiving;
using BKMC.Pharmacy.Web.Models.Suppliers;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Receiving;

public class CreateModel : PageModel
{
    private readonly ReceivingApiService _receivingApiService;
    private readonly MedicineApiService _medicineApiService;
    private readonly SupplierApiService _supplierApiService;

    public CreateModel(
        ReceivingApiService receivingApiService,
        MedicineApiService medicineApiService,
        SupplierApiService supplierApiService)
    {
        _receivingApiService = receivingApiService;
        _medicineApiService = medicineApiService;
        _supplierApiService = supplierApiService;
    }

    [BindProperty]
    public CreateReceivingModel Receiving { get; set; } = new();

    public List<MedicineDto> Medicines { get; set; } = new();
    public List<SupplierDto> Suppliers { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadMedicinesAsync();
        await LoadSuppliersAsync();

        if (Receiving.Items.Count == 0)
        {
            Receiving.Items.Add(new ReceivingItemModel());
        }

        if (Receiving.InvoiceDate == default)
        {
            Receiving.InvoiceDate = DateTime.Today;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadMedicinesAsync();
        await LoadSuppliersAsync();

        if (Receiving.Items == null ||
            Receiving.Items.Count == 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "At least one receiving item is required.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _receivingApiService.CreateAsync(Receiving);

            TempData["SuccessMessage"] =
                "Medicine receiving created successfully.";

            return RedirectToPage("/Receiving/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage =
                ex.Message;

            return Page();
        }
    }

    private async Task LoadMedicinesAsync()
    {
        try
        {
            Medicines =
                await _medicineApiService.GetAllAsync(
                    null,
                    true);
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Unable to load medicines. {ex.Message}";

            Medicines = new List<MedicineDto>();
        }
    }

    private async Task LoadSuppliersAsync()
    {
        try
        {
            Suppliers =
                await _supplierApiService.GetAllAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Unable to load suppliers. {ex.Message}";

            Suppliers = new List<SupplierDto>();
        }
    }
}