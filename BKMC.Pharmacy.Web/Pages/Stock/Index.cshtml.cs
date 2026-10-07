using BKMC.Pharmacy.Web.Models.Stock;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Stock;

public class IndexModel : PageModel
{
    private readonly StockApiService _stockApiService;

    public IndexModel(StockApiService stockApiService)
    {
        _stockApiService = stockApiService;
    }

    public List<StockDto> Stocks { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public string? SearchTerm { get; set; }

    public StockLocation? SelectedLocation { get; set; }

    public async Task OnGetAsync(
        string? searchTerm,
        StockLocation? selectedLocation)
    {
        SearchTerm = searchTerm;
        SelectedLocation = selectedLocation;

        try
        {
            var stocks = await _stockApiService.GetAllAsync();

            Stocks = stocks;

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var search = SearchTerm.Trim();

                Stocks = Stocks
                    .Where(x =>
                        x.MedicineName.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        x.BatchNumber.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (SelectedLocation.HasValue)
            {
                Stocks = Stocks
                    .Where(x =>
                        x.Location == SelectedLocation.Value)
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Unable to load stock data. {ex.Message}";
        }
    }
}