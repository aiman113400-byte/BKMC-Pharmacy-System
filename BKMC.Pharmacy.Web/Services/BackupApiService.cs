using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BKMC.Pharmacy.Web.Services;

public class BackupApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BackupApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<BackupHistoryDto>> GetHistoryAsync()
    {
        var client = CreateClient();

        var response =
            await client.GetAsync("api/Backup/history");

        if (!response.IsSuccessStatusCode)
        {
            return new List<BackupHistoryDto>();
        }

        return await response.Content
            .ReadFromJsonAsync<List<BackupHistoryDto>>()
            ?? new List<BackupHistoryDto>();
    }

    public async Task<BackupResultDto?> CreateBackupAsync()
    {
        var client = CreateClient();

        var response =
            await client.PostAsync("api/Backup/create", null);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content
            .ReadFromJsonAsync<BackupResultDto>();
    }

    private HttpClient CreateClient()
    {
        var client =
            _httpClientFactory.CreateClient("BKMC.Pharmacy.API");

        var token =
            _httpContextAccessor.HttpContext?
                .Session.GetString("JWToken");

        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);
        }

        return client;
    }
}

public class BackupHistoryDto
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? CreatedBy { get; set; }

    public string? ErrorMessage { get; set; }
}

public class BackupResultDto
{
    public string Message { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string BackupPath { get; set; } = string.Empty;
}