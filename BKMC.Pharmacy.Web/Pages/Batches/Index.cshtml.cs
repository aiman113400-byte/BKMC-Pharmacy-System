using BKMC.Pharmacy.Web.Models.Batches;
using BKMC.Pharmacy.Web.Models.Medicines;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Batches;

public class IndexModel : PageModel
{
    private readonly BatchApiService _batchApiService;
    private readonly MedicineApiService _medicineApiService;

    public IndexModel(
        BatchApiService batchApiService,
        MedicineApiService medicineApiService)
    {
        _batchApiService = batchApiService;
        _medicineApiService = medicineApiService;
    }

    public List<BatchDto> Batches { get; set; } = new();
    public List<MedicineDto> Medicines { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            // 1. Load all batches
            var allBatches = await _batchApiService.GetAllAsync();

            // 2. Filter by medicine name or batch number
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.Trim().ToLower();

                allBatches = allBatches
                    .Where(b =>
                        (b.MedicineName ?? "").ToLower().Contains(term) ||
                        (b.BatchNumber ?? "").ToLower().Contains(term))
                    .ToList();
            }

            Batches = allBatches;

            // 3. Load medicines (optional)
            Medicines = await _medicineApiService.GetAllAsync(null, true);

            return Page();
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to connect to the Medicine or Batch API.";
            return Page();
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] =
                "An error occurred while loading batch information.";
            return Page();
        }
    }
}