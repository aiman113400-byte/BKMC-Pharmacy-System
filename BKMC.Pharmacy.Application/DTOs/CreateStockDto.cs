using BKMC.Pharmacy.Domain.Enums;

namespace BKMC.Pharmacy.Application.DTOs.Stock;

public class CreateStockDto
{
    public int MedicineId { get; set; }

    public int BatchId { get; set; }

    public StockLocation Location { get; set; }

    public decimal Quantity { get; set; }
}

public class StockDto
{
    public int Id { get; set; }

    public int MedicineId { get; set; }

    public string MedicineName { get; set; } = string.Empty;

    public int BatchId { get; set; }

    public string BatchNumber { get; set; } = string.Empty;

    public StockLocation Location { get; set; }

    public decimal Quantity { get; set; }

    public DateTime UpdatedAt { get; set; }
}