using BKMC.Pharmacy.Web.Models.Batches;
using BKMC.Pharmacy.Web.Models.Medicines;
using BKMC.Pharmacy.Web.Models.Stock;
using BKMC.Pharmacy.Web.Models.StockAdjustments;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.StockAdjustment;

public class CreateModel : PageModel
{
    private readonly StockApiService _stockApiService;
    private readonly MedicineApiService _medicineApiService;
    private readonly BatchApiService _batchApiService;
    private readonly StockAdjustmentApiService _stockAdjustmentApiService;

    public CreateModel(
        StockApiService stockApiService,
        MedicineApiService medicineApiService,
        BatchApiService batchApiService,
        StockAdjustmentApiService stockAdjustmentApiService)
    {
        _stockApiService = stockApiService;
        _medicineApiService = medicineApiService;
        _batchApiService = batchApiService;
        _stockAdjustmentApiService = stockAdjustmentApiService;
    }

    [BindProperty]
    public CreateStockAdjustmentModel Adjustment { get; set; } = new();

    public List<MedicineDto> Medicines { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Medicines = await _medicineApiService.GetAllAsync(null, true);
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to load medicines. {ex.Message}";
            return Page();
        }
    }

    // ✅ YEH HANDLER ZAROORI HAI — Batch dropdown ke liye
    public async Task<JsonResult> OnGetBatchesAsync(int medicineId)
    {
        try
        {
            if (medicineId <= 0)
            {
                return new JsonResult(new List<object>());
            }

            // Batch API se us medicine ke batches load karo
            var batches = await _batchApiService.GetByMedicineIdAsync(medicineId);

            // Sirf zaroori fields return karo (JavaScript ke liye)
            var result = batches
                .Where(b => b != null)
                .Select(b => new
                {
                    id = b.Id,
                    batchNumber = b.BatchNumber,
                    expiryDate = b.ExpiryDate
                })
                .ToList();

            return new JsonResult(result);
        }
        catch (Exception ex)
        {
            // Log error aur empty list return
            Console.WriteLine($"Error loading batches: {ex.Message}");
            return new JsonResult(new List<object>());
        }
    }
    // ✅ Current Quantity load karne ke liye handler
    public async Task<JsonResult> OnGetCurrentQuantityAsync(
        int medicineId, int batchId, int location)
    {
        try
        {
            if (medicineId <= 0 || batchId <= 0)
            {
                return new JsonResult(new { quantity = 0 });
            }

            // Stock API se current quantity load karo
            var stocks = await _stockApiService.GetAllAsync();

            // Filter: medicine + batch + location
            var stock = stocks.FirstOrDefault(s =>
                s.MedicineId == medicineId &&
                s.BatchId == batchId &&
                (int)s.Location == location);

            var quantity = stock?.Quantity ?? 0;

            return new JsonResult(new { quantity = quantity });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading current quantity: {ex.Message}");
            return new JsonResult(new { quantity = 0 });
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Medicines = await _medicineApiService.GetAllAsync(null, true);
            return Page();
        }

        try
        {
            var result =
                await _stockAdjustmentApiService.CreateAsync(Adjustment);

            if (result == null)
            {
                ErrorMessage =
                    "Stock adjustment failed. Please try again.";

                Medicines =
                    await _medicineApiService.GetAllAsync(null, true);

                return Page();
            }

            return RedirectToPage("/Stock/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Unable to adjust stock. {ex.Message}";

            Medicines =
                await _medicineApiService.GetAllAsync(null, true);

            return Page();
        }
    }
}



//using BKMC.Pharmacy.Web.Models.Batches;
//using BKMC.Pharmacy.Web.Models.Medicines;
//using BKMC.Pharmacy.Web.Models.StockAdjustments;
//using BKMC.Pharmacy.Web.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//namespace BKMC.Pharmacy.Web.Pages.StockAdjustment;

//public class CreateModel : PageModel
//{
//    private readonly StockAdjustmentApiService _stockAdjustmentApiService;
//    private readonly MedicineApiService _medicineApiService;
//    private readonly BatchApiService _batchApiService;

//    public CreateModel(
//        StockAdjustmentApiService stockAdjustmentApiService,
//        MedicineApiService medicineApiService,
//        BatchApiService batchApiService)
//    {
//        _stockAdjustmentApiService = stockAdjustmentApiService;
//        _medicineApiService = medicineApiService;
//        _batchApiService = batchApiService;
//    }

//    [BindProperty]
//    public CreateStockAdjustmentModel Adjustment { get; set; } = new();

//    public List<MedicineDto> Medicines { get; set; } = new();
//    public List<BatchLookupDto> Batches { get; set; } = new();

//    public string? ErrorMessage { get; set; }

//    public async Task OnGetAsync()
//    {
//        Adjustment.AdjustmentDate = DateTime.Now;

//        try
//        {
//            Medicines = await _medicineApiService.GetAllAsync();
//        }
//        catch (Exception ex)
//        {
//            ErrorMessage =
//                $"Unable to load medicines. {ex.Message}";
//        }
//    }
//    public async Task<IActionResult> OnGetBatchesAsync(int medicineId)
//    {
//        if (medicineId <= 0)
//        {
//            return new JsonResult(new List<BatchLookupDto>());
//        }

//        try
//        {
//            var batches =
//                await _batchApiService.GetByMedicineIdAsync(medicineId);

//            return new JsonResult(batches);
//        }
//        catch
//        {
//            return new JsonResult(new List<BatchLookupDto>());
//        }
//    }
//}