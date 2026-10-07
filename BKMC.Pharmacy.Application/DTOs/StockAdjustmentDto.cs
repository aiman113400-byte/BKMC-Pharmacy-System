using BKMC.Pharmacy.Domain.Enums;

namespace BKMC.Pharmacy.Application.DTOs.StockAdjustments;

public class StockAdjustmentDto
{
    public int Id { get; set; }

    public int MedicineId { get; set; }

    public int BatchId { get; set; }

    public StockLocation Location { get; set; }

    public decimal PreviousQuantity { get; set; }

    public decimal NewQuantity { get; set; }

    public decimal Difference { get; set; }

    public string? Reason { get; set; }

    public DateTime AdjustmentDate { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateStockAdjustmentDto
{
    public int MedicineId { get; set; }

    public int BatchId { get; set; }

    public StockLocation Location { get; set; }

    public decimal NewQuantity { get; set; }

    public string? Reason { get; set; }

    public DateTime AdjustmentDate { get; set; }
}