using BKMC.Pharmacy.Web.Models.StockTransfers;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.StockTransfers;

public class IndexModel : PageModel
{
    private readonly StockTransferApiService _stockTransferApiService;

    public IndexModel(
        StockTransferApiService stockTransferApiService)
    {
        _stockTransferApiService = stockTransferApiService;
    }

    public List<StockTransferDto> Transfers { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            Transfers =
                await _stockTransferApiService.GetAllAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Unable to load stock transfers. {ex.Message}";
        }
    }
}