using BKMC.Pharmacy.Web.Models.Suppliers;
using System.Net.Http.Json;

namespace BKMC.Pharmacy.Web.Services;

public class SupplierApiService
{
    private readonly HttpClient _httpClient;

    public SupplierApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SupplierDto>> GetAllAsync()
    {
        var response =
            await _httpClient.GetAsync("api/Suppliers");

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new Exception(
                $"Unable to load suppliers. {error}");
        }

        var suppliers =
            await response.Content
                .ReadFromJsonAsync<List<SupplierDto>>();

        return suppliers ?? new List<SupplierDto>();
    }
}