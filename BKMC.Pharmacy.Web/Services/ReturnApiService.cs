using System.Net;
using System.Net.Http.Json;
using BKMC.Pharmacy.Web.Models.Returns;

namespace BKMC.Pharmacy.Web.Services;

public class ReturnApiService
{
    private readonly HttpClient _httpClient;

    public ReturnApiService(
        IHttpClientFactory httpClientFactory)
    {
        _httpClient =
            httpClientFactory.CreateClient(
                "BKMC.Pharmacy.API");
    }

    public async Task<List<ReturnDto>> GetAllAsync()
    {
        var returns =
            await _httpClient.GetFromJsonAsync<List<ReturnDto>>(
                "api/Returns");

        return returns ?? new List<ReturnDto>();
    }

    public async Task<ReturnDto?> GetByIdAsync(int id)
    {
        var response =
            await _httpClient.GetAsync(
                $"api/Returns/{id}");

        if (response.StatusCode ==
            HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ReturnDto>();
    }

    public async Task<ReturnDto?> CreateAsync(
        CreateReturnModel model)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/Returns",
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
            .ReadFromJsonAsync<ReturnDto>();
    }
}