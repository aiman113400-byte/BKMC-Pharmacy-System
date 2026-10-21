using System.Net.Http.Json;


namespace BKMC.Pharmacy.Web.Services;

public class ReportsApiService
{
    private readonly HttpClient _httpClient;

    public ReportsApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("BKMC.Pharmacy.API");
    }

    public async Task<StockSummaryReportDto?> GetStockSummaryAsync()
    {
        return await _httpClient.GetFromJsonAsync<StockSummaryReportDto>(
            "api/Reports/stock-summary");
    }

    public async Task<List<LowStockReportDto>> GetLowStockReportAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<LowStockReportDto>>(
            "api/Reports/low-stock")
            ?? new List<LowStockReportDto>();
    }

    public async Task<DailySalesReportDto?> GetDailySalesAsync(DateTime date)
    {
        var formattedDate = date.ToString("yyyy-MM-dd");

        return await _httpClient.GetFromJsonAsync<DailySalesReportDto>(
            $"api/Reports/daily-sales?date={formattedDate}");
    }

    public async Task<MonthlySalesReportDto?> GetMonthlySalesAsync(
    int year,
    int month)
    {
        return await _httpClient.GetFromJsonAsync<MonthlySalesReportDto>(
            $"api/Reports/monthly-sales?year={year}&month={month}");
    }
}

public class StockSummaryReportDto
{
    public int TotalStockRecords { get; set; }

    public decimal TotalWarehouseQuantity { get; set; }

    public decimal TotalPharmacyQuantity { get; set; }

    public decimal TotalQuantity { get; set; }

    public List<StockReportItemDto> StockItems { get; set; } = new();
}

public class StockReportItemDto
{
    public int Id { get; set; }

    public int MedicineId { get; set; }

    public string MedicineName { get; set; } = string.Empty;

    public int BatchId { get; set; }

    public string BatchNumber { get; set; } = string.Empty;

    public int Location { get; set; }

    public decimal Quantity { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class LowStockReportDto
{
    public int MedicineId { get; set; }

    public string MedicineCode { get; set; } = string.Empty;

    public string MedicineName { get; set; } = string.Empty;

    public decimal ReorderLevel { get; set; }

    public decimal WarehouseQuantity { get; set; }

    public decimal PharmacyQuantity { get; set; }

    public decimal TotalQuantity { get; set; }
}

public class DailySalesReportDto
{
    public DateTime Date { get; set; }

    public int TotalBills { get; set; }

    public decimal GrossSales { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public decimal MedicinesSold { get; set; }

    public List<DailySalesBillDto> Bills { get; set; } = new();
}

public class DailySalesBillDto
{
    public int BillId { get; set; }

    public string BillNumber { get; set; } = string.Empty;

    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public DateTime BillDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public decimal MedicinesSold { get; set; }
}

public class MonthlySalesReportDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public int TotalBills { get; set; }

    public decimal GrossSales { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public decimal MedicinesSold { get; set; }

    public List<MonthlySalesDayDto> DailySales { get; set; } = new();
}

public class MonthlySalesDayDto
{
    public DateTime Date { get; set; }

    public int TotalBills { get; set; }

    public decimal GrossSales { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public decimal MedicinesSold { get; set; }
}