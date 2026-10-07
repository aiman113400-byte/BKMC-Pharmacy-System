using System.Net.Http.Json;
using BKMC.Pharmacy.Web.Models.Patients;

namespace BKMC.Pharmacy.Web.Services;

public class PatientApiService
{
    private readonly HttpClient _httpClient;

    public PatientApiService(
        IHttpClientFactory httpClientFactory)
    {
        _httpClient =
            httpClientFactory.CreateClient("BKMC.Pharmacy.API");
    }

    public async Task<List<PatientDto>> GetAllAsync()
    {
        var patients =
            await _httpClient.GetFromJsonAsync<List<PatientDto>>(
                "api/Patients");

        return patients ?? new List<PatientDto>();
    }

    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<PatientDto>(
            $"api/Patients/{id}");
    }

    public async Task<PatientDto?> GetByMRNumberAsync(
        string mrNumber)
    {
        return await _httpClient.GetFromJsonAsync<PatientDto>(
            $"api/Patients/lookup?mrNumber={Uri.EscapeDataString(mrNumber)}");
    }

    public async Task<PatientDto?> CreateAsync(
        CreatePatientModel model)
    {
        var response =
            await _httpClient.PostAsJsonAsync(
                "api/Patients",
                model);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<PatientDto>();
    }

    public async Task<PatientDto?> UpdateAsync(
        int id,
        UpdatePatientModel model)
    {
        var response =
            await _httpClient.PutAsJsonAsync(
                $"api/Patients/{id}",
                model);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<PatientDto>();
    }
}