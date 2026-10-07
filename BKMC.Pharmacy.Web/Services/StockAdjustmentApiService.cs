using System.Net.Http.Json;
using BKMC.Pharmacy.Web.Models.StockAdjustments;


namespace BKMC.Pharmacy.Web.Services;

public class StockAdjustmentApiService
{
    private readonly HttpClient _httpClient;

    public StockAdjustmentApiService(
        IHttpClientFactory httpClientFactory)
    {
        _httpClient =
            httpClientFactory.CreateClient("BKMC.Pharmacy.API");
    }

    public async Task<List<StockAdjustmentDto>> GetAllAsync()
    {
        var adjustments =
            await _httpClient.GetFromJsonAsync<List<StockAdjustmentDto>>(
                "api/StockAdjustments");

        return adjustments ?? new List<StockAdjustmentDto>();
    }

    public async Task<StockAdjustmentDto?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<StockAdjustmentDto>(
            $"api/StockAdjustments/{id}");
    }

    public async Task<StockAdjustmentDto?> CreateAsync(
        CreateStockAdjustmentModel model)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/StockAdjustments",
            model);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<StockAdjustmentDto>();
    }
}