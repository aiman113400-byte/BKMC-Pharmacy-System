
using BKMC.Pharmacy.Application.DTOs.Stock;

namespace BKMC.Pharmacy.Application.DTOs.Reports;

public class StockSummaryReportDto
{
    public int TotalStockRecords { get; set; }

    public decimal TotalWarehouseQuantity { get; set; }

    public decimal TotalPharmacyQuantity { get; set; }

    public decimal TotalQuantity { get; set; }

    public List<StockDto> StockItems { get; set; } = new();
}
