using System.Net;
using System.Net.Http.Json;
using BKMC.Pharmacy.Web.Models.Medicines;

namespace BKMC.Pharmacy.Web.Services;

public class MedicineApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MedicineApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient("BKMC.Pharmacy.API");
    }

    // GET: /api/Medicines
    public async Task<List<MedicineDto>> GetAllAsync(
        string? search = null,
        bool? isActive = null)
    {
        var client = CreateClient();

        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add($"search={Uri.EscapeDataString(search)}");
        }

        if (isActive.HasValue)
        {
            query.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");
        }

        var url = "api/Medicines";

        if (query.Count > 0)
        {
            url += "?" + string.Join("&", query);
        }

        var medicines = await client.GetFromJsonAsync<List<MedicineDto>>(url);

        return medicines ?? new List<MedicineDto>();
    }

    // GET: /api/Medicines/{id}
    public async Task<MedicineDto?> GetByIdAsync(int id)
    {
        var client = CreateClient();

        var response = await client.GetAsync($"api/Medicines/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<ApiResponse<MedicineDto>>();

        return result?.Data;
    }

    // POST: /api/Medicines
    public async Task<MedicineDto> CreateAsync(
        CreateMedicineModel model)
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "api/Medicines",
            model);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<ApiResponse<MedicineDto>>();

        if (result?.Data == null)
        {
            throw new InvalidOperationException(
                result?.Message ?? "Medicine could not be created.");
        }

        return result.Data;
    }

    // PUT: /api/Medicines/{id}
    public async Task<MedicineDto?> UpdateAsync(
        int id,
        UpdateMedicineModel model)
    {
        var client = CreateClient();

        var response = await client.PutAsJsonAsync(
            $"api/Medicines/{id}",
            model);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<ApiResponse<MedicineDto>>();

        return result?.Data;
    }

    // PATCH: /api/Medicines/{id}/activate
    public async Task<bool> ActivateAsync(int id)
    {
        var client = CreateClient();

        var response = await client.PatchAsync(
            $"api/Medicines/{id}/activate",
            null);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();

        return true;
    }

    // PATCH: /api/Medicines/{id}/deactivate
    public async Task<bool> DeactivateAsync(int id)
    {
        var client = CreateClient();

        var response = await client.PatchAsync(
            $"api/Medicines/{id}/deactivate",
            null);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();

        return true;
    }

    private class ApiResponse<T>
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public T? Data { get; set; }
    }
}