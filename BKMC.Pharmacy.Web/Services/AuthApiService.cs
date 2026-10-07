using System.Net.Http.Json;
using BKMC.Pharmacy.Web.Models.Auth;

namespace BKMC.Pharmacy.Web.Services;

public class AuthApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<LoginResponseDto?> LoginAsync(
        LoginRequestDto request)
    {
        var client =
            _httpClientFactory.CreateClient("BKMC.Pharmacy.API");

        var response = await client.PostAsJsonAsync(
            "api/Auth/login",
            request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<LoginResponseDto>();
    }
}