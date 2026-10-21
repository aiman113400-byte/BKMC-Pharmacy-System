
using BKMC.Pharmacy.Application.DTOs.Reports;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IReportService
{
    Task<StockSummaryReportDto> GetStockSummaryAsync();

    Task<List<LowStockReportDto>> GetLowStockReportAsync();

    Task<DailySalesReportDto> GetDailySalesAsync(DateTime date);
    Task<MonthlySalesReportDto> GetMonthlySalesAsync(int year, int month);
}
