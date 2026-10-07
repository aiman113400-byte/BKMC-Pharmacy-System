using BKMC.Pharmacy.Web.Models.Batches;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Batches;

public class MedicineModel : PageModel
{
    private readonly BatchApiService _batchApiService;

    public MedicineModel(BatchApiService batchApiService)
    {
        _batchApiService = batchApiService;
    }

    public List<BatchLookupDto> Batches { get; set; } = new();

    public List<BatchLookupDto> AvailableBatches { get; set; } = new();


    public int AvailableBatchCount => AvailableBatches.Count;

    public int MedicineId { get; set; }

    public async Task<IActionResult> OnGetAsync(int medicineId)
    {
        if (medicineId <= 0)
        {
            return BadRequest();
        }

        MedicineId = medicineId;

        try
        {
            Batches = await _batchApiService
                .GetByMedicineIdAsync(medicineId);

            AvailableBatches = await _batchApiService
                .GetAvailableBatchesAsync(medicineId);

            return Page();
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] =
                "Unable to connect to the Batch API.";

            return Page();
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] =
                "An error occurred while loading medicine batches.";

            return Page();
        }
    }
}











//using BKMC.Pharmacy.Web.Models.Batches;
//using BKMC.Pharmacy.Web.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//namespace BKMC.Pharmacy.Web.Pages.Batches;

//public class MedicineModel : PageModel
//{
//    private readonly BatchApiService _batchApiService;

//    public MedicineModel(BatchApiService batchApiService)
//    {
//        _batchApiService = batchApiService;
//    }

//    public List<BatchLookupDto> Batches { get; set; } = new();

//    public int MedicineId { get; set; }

//    public async Task<IActionResult> OnGetAsync(int medicineId)
//    {
//        if (medicineId <= 0)
//        {
//            return BadRequest();
//        }

//        MedicineId = medicineId;

//        try
//        {
//            Batches = await _batchApiService
//                .GetByMedicineIdAsync(medicineId);

//            return Page();
//        }
//        catch (HttpRequestException)
//        {
//            TempData["ErrorMessage"] =
//                "Unable to connect to the Batch API.";

//            return Page();
//        }
//        catch (Exception)
//        {
//            TempData["ErrorMessage"] =
//                "An error occurred while loading medicine batches.";

//            return Page();
//        }
//    }
//}