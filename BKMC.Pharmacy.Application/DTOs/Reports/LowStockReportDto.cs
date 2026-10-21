namespace BKMC.Pharmacy.Application.DTOs.Reports;

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