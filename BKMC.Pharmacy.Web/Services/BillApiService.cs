using System.Net;
using System.Net.Http.Json;
using BKMC.Pharmacy.Web.Models.Billing;

namespace BKMC.Pharmacy.Web.Services;

public class BillApiService
{
    private readonly HttpClient _httpClient;

    public BillApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient =
            httpClientFactory.CreateClient("BKMC.Pharmacy.API");
    }

    public async Task<List<BillDto>> GetAllAsync()
    {
        var bills =
            await _httpClient.GetFromJsonAsync<List<BillDto>>(
                "api/Bills");

        return bills ?? new List<BillDto>();
    }

    public async Task<BillDto?> GetByIdAsync(int id)
    {
        var response =
            await _httpClient.GetAsync(
                $"api/Bills/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<BillDto>();
    }

    public async Task<BillDto?> CreateAsync(
        CreateBillModel model)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/Bills",
                model);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(error)
                    ? $"API returned status code {(int)response.StatusCode}."
                    : error);
        }

        return await response.Content
            .ReadFromJsonAsync<BillDto>();
    }
}