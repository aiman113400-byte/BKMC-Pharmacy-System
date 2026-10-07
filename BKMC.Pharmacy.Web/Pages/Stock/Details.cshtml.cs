using BKMC.Pharmacy.Web.Models.Stock;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Stock;

public class DetailsModel : PageModel
{
    private readonly StockApiService _stockApiService;

    public DetailsModel(StockApiService stockApiService)
    {
        _stockApiService = stockApiService;
    }

    public StockDto? Stock { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            Stock = await _stockApiService.GetByIdAsync(id);

            if (Stock == null)
            {
                return NotFound();
            }

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to load stock details. {ex.Message}";

            return Page();
        }
    }
}