
using System.Net;
using System.Net.Http.Json;
using BKMC.Pharmacy.Web.Models.Suppliers;

namespace BKMC.Pharmacy.Web.Services;

public class SupplierApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SupplierApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient("BKMC.Pharmacy.API");
    }

    // GET: api/Suppliers
    public async Task<List<SupplierDto>> GetAllAsync()
    {
        var client = CreateClient();

        var suppliers = await client.GetFromJsonAsync<List<SupplierDto>>(
            "api/Suppliers");

        return suppliers ?? new List<SupplierDto>();
    }

    // GET: api/Suppliers/search?search=...
    public async Task<List<SupplierDto>> SearchAsync(string? search)
    {
        var client = CreateClient();

        var url = "api/Suppliers/search";

        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"?search={Uri.EscapeDataString(search.Trim())}";
        }

        var suppliers = await client.GetFromJsonAsync<List<SupplierDto>>(url);

        return suppliers ?? new List<SupplierDto>();
    }

    // GET: api/Suppliers/{id}
    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        var client = CreateClient();

        var response = await client.GetAsync($"api/Suppliers/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SupplierDto>();
    }

    // POST: api/Suppliers
    public async Task<SupplierDto> CreateAsync(CreateSupplierModel model)
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "api/Suppliers",
            model);

        response.EnsureSuccessStatusCode();

        var supplier = await response.Content.ReadFromJsonAsync<SupplierDto>();

        return supplier
            ?? throw new InvalidOperationException(
                "Supplier was created, but the response was empty.");
    }

    // PUT: api/Suppliers/{id}
    public async Task<SupplierDto?> UpdateAsync(
        int id,
        UpdateSupplierModel model)
    {
        var client = CreateClient();

        var response = await client.PutAsJsonAsync(
            $"api/Suppliers/{id}",
            model);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SupplierDto>();
    }

    // PATCH: api/Suppliers/{id}/activate
    public async Task<bool> ActivateAsync(int id)
    {
        var client = CreateClient();

        var response = await client.PatchAsync(
            $"api/Suppliers/{id}/activate",
            null);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();

        return true;
    }

    // PATCH: api/Suppliers/{id}/deactivate
    public async Task<bool> DeactivateAsync(int id)
    {
        var client = CreateClient();

        var response = await client.PatchAsync(
            $"api/Suppliers/{id}/deactivate",
            null);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();

        return true;
    }
}



//using BKMC.Pharmacy.Web.Models.Suppliers;
//using System.Net.Http.Json;

//namespace BKMC.Pharmacy.Web.Services;

//public class SupplierApiService
//{
//    private readonly HttpClient _httpClient;

//    public SupplierApiService(HttpClient httpClient)
//    {
//        _httpClient = httpClient;
//    }

//    public async Task<List<SupplierDto>> GetAllAsync()
//    {
//        var response =
//            await _httpClient.GetAsync("api/Suppliers");

//        if (!response.IsSuccessStatusCode)
//        {
//            var error =
//                await response.Content.ReadAsStringAsync();

//            throw new Exception(
//                $"Unable to load suppliers. {error}");
//        }

//        var suppliers =
//            await response.Content
//                .ReadFromJsonAsync<List<SupplierDto>>();

//        return suppliers ?? new List<SupplierDto>();
//    }
//}