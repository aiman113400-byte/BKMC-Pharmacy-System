using System.Net;
using System.Net.Http.Json;
using BKMC.Pharmacy.Web.Models.Dispensings;

namespace BKMC.Pharmacy.Web.Services;

public class DispensingApiService
{
    private readonly HttpClient _httpClient;

    public DispensingApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient =
            httpClientFactory.CreateClient("BKMC.Pharmacy.API");
    }

    public async Task<List<DispensingDto>> GetAllAsync()
    {
        var dispensings =
            await _httpClient.GetFromJsonAsync<List<DispensingDto>>(
                "api/Dispensings");

        return dispensings ?? new List<DispensingDto>();
    }

    public async Task<DispensingDto?> GetByIdAsync(int id)
    {
        var response =
            await _httpClient.GetAsync($"api/Dispensings/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<DispensingDto>();
    }

    public async Task<DispensingDto?> CreateAsync(
        CreateDispensingModel model)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/Dispensings",
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
            .ReadFromJsonAsync<DispensingDto>();
    }
}