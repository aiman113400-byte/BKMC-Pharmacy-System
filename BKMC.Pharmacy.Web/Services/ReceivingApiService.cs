using BKMC.Pharmacy.Web.Models.Receiving;
using System.Net.Http.Json;

namespace BKMC.Pharmacy.Web.Services;

public class ReceivingApiService
{
    private readonly HttpClient _httpClient;

    public ReceivingApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Get all receiving records
    public async Task<List<ReceivingDto>> GetAllAsync()
    {
        var response =
            await _httpClient.GetAsync("api/Receivings");

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new Exception(
                $"Unable to load receivings. {error}");
        }

        var receivings =
            await response.Content
                .ReadFromJsonAsync<List<ReceivingDto>>();

        return receivings ?? new List<ReceivingDto>();
    }

    // Get receiving by ID
    public async Task<ReceivingDto?> GetByIdAsync(int id)
    {
        var response =
            await _httpClient.GetAsync(
                $"api/Receivings/{id}");

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new Exception(
                $"Unable to load receiving. {error}");
        }

        return await response.Content
            .ReadFromJsonAsync<ReceivingDto>();
    }

    // Create receiving
    public async Task<ReceivingDto> CreateAsync(
        CreateReceivingModel model)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/Receivings",
                model);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            throw new Exception(
                $"Unable to create receiving. {error}");
        }

        var receiving =
            await response.Content
                .ReadFromJsonAsync<ReceivingDto>();

        if (receiving == null)
        {
            throw new Exception(
                "Invalid response received from receiving API.");
        }

        return receiving;
    }
}