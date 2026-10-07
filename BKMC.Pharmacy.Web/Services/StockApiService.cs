using BKMC.Pharmacy.Web.Models.Stock;

namespace BKMC.Pharmacy.Web.Services;

public class StockApiService
{
    private readonly HttpClient _httpClient;

    public StockApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("BKMC.Pharmacy.API");
    }

    public async Task<List<StockDto>> GetAllAsync()
    {
        var stocks = await _httpClient.GetFromJsonAsync<List<StockDto>>(
            "api/Stocks");

        return stocks ?? new List<StockDto>();
    }

    public async Task<StockDto?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<StockDto>(
            $"api/Stocks/{id}");
    }
}