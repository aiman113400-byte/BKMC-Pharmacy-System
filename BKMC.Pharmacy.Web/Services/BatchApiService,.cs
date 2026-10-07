using BKMC.Pharmacy.Web.Models.Batches;

namespace BKMC.Pharmacy.Web.Services;

public class BatchApiService
{
    private readonly HttpClient _httpClient;

    public BatchApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // GET: api/Batches
    public async Task<List<BatchDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/Batches");

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<BatchDto>>()
            ?? new List<BatchDto>();
    }

    // GET: api/Batches/{id}
    public async Task<BatchDto?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync(
            $"api/Batches/{id}");

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<BatchDto>();
    }

    // GET: api/Batches/medicine/{medicineId}
    public async Task<List<BatchLookupDto>> GetByMedicineIdAsync(
        int medicineId)
    {
        var response = await _httpClient.GetAsync(
            $"api/Batches/medicine/{medicineId}");

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<BatchLookupDto>>()
            ?? new List<BatchLookupDto>();
    }

    // GET: api/Batches/medicine/{medicineId}/available
    public async Task<List<BatchLookupDto>> GetAvailableBatchesAsync(
        int medicineId)
    {
        var response = await _httpClient.GetAsync(
            $"api/Batches/medicine/{medicineId}/available");

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<BatchLookupDto>>()
            ?? new List<BatchLookupDto>();
    }
}