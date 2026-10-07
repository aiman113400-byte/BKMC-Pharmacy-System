using BKMC.Pharmacy.Web.Models.Medicines;
using BKMC.Pharmacy.Web.Models.Stock;
using BKMC.Pharmacy.Web.Models.StockTransfers;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.StockTransfers;

public class CreateModel : PageModel
{
    private readonly StockTransferApiService _stockTransferApiService;
    private readonly StockApiService _stockApiService;
    private readonly MedicineApiService _medicineApiService;

    public CreateModel(
        StockTransferApiService stockTransferApiService,
        StockApiService stockApiService,
        MedicineApiService medicineApiService)
    {
        _stockTransferApiService = stockTransferApiService;
        _stockApiService = stockApiService;
        _medicineApiService = medicineApiService;
    }

    [BindProperty]
    public CreateStockTransferDto Transfer { get; set; } = new();

    public List<StockDto> Stocks { get; set; } = new();

    public List<MedicineDto> Medicines { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        Transfer.TransferDate = DateTime.Today;

        Transfer.FromLocation = StockLocation.Warehouse;
        Transfer.ToLocation = StockLocation.Pharmacy;

        if (Transfer.Items.Count == 0)
        {
            Transfer.Items.Add(
                new CreateStockTransferItemDto());
        }

        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDataAsync();

        if (Transfer.FromLocation == Transfer.ToLocation)
        {
            ModelState.AddModelError(
                string.Empty,
                "From Location and To Location cannot be the same.");
        }

        if (Transfer.Items == null ||
            Transfer.Items.Count == 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "At least one transfer item is required.");
        }

        if (Transfer.Items != null)
        {
            foreach (var item in Transfer.Items)
            {
                if (item.MedicineId <= 0)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Please select a medicine for every item.");
                }

                if (item.BatchId <= 0)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Please select a batch for every item.");
                }

                if (item.Quantity <= 0)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Transfer quantity must be greater than zero.");
                }
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _stockTransferApiService.CreateAsync(Transfer);

            TempData["SuccessMessage"] =
                "Stock transfer created successfully.";

            return RedirectToPage("/StockTransfers/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;

            return Page();
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            Stocks = await _stockApiService.GetAllAsync();

            Medicines = await _medicineApiService.GetAllAsync(
                null,
                true);

            // Transfer source ke liye sirf Warehouse stock
            Stocks = Stocks
                .Where(x => x.Location == StockLocation.Warehouse)
                .ToList();
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Unable to load transfer data. {ex.Message}";

            Stocks = new List<StockDto>();
            Medicines = new List<MedicineDto>();
        }
    }
}