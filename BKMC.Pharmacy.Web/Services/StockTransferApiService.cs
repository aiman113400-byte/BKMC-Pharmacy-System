using System.Net.Http.Json;
using BKMC.Pharmacy.Web.Models.StockTransfers;

namespace BKMC.Pharmacy.Web.Services;

public class StockTransferApiService
{
    private readonly HttpClient _httpClient;

    public StockTransferApiService(
        IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(
            "BKMC.Pharmacy.API");
    }

    public async Task<List<StockTransferDto>> GetAllAsync()
    {
        var transfers =
            await _httpClient.GetFromJsonAsync<List<StockTransferDto>>(
                "api/StockTransfers");

        return transfers ?? new List<StockTransferDto>();
    }

    public async Task<StockTransferDto?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<StockTransferDto>(
            $"api/StockTransfers/{id}");
    }

    public async Task<StockTransferDto> CreateAsync(
        CreateStockTransferDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/StockTransfers",
            dto);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                string.IsNullOrWhiteSpace(error)
                    ? "Unable to create stock transfer."
                    : error);
        }

        var transfer =
            await response.Content
                .ReadFromJsonAsync<StockTransferDto>();

        if (transfer == null)
        {
            throw new InvalidOperationException(
                "Stock transfer response was empty.");
        }

        return transfer;
    }
}