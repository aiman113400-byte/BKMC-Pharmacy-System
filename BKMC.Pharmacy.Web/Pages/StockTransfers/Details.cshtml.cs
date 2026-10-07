using BKMC.Pharmacy.Web.Models.StockTransfers;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.StockTransfers;

public class DetailsModel : PageModel
{
    private readonly StockTransferApiService _stockTransferApiService;

    public DetailsModel(StockTransferApiService stockTransferApiService)
    {
        _stockTransferApiService = stockTransferApiService;
    }

    public StockTransferDto? Transfer { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        try
        {
            Transfer = await _stockTransferApiService.GetByIdAsync(id);

            if (Transfer == null)
            {
                return NotFound();
            }

            return Page();
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Unable to connect to the Stock Transfer API.";
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred while loading transfer details. {ex.Message}";
            return Page();
        }
    }
}