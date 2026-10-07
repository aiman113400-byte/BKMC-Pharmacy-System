using BKMC.Pharmacy.Domain.Enums;

namespace BKMC.Pharmacy.Domain.Entities;

public class Stock
{
    public int Id { get; set; }

    public int MedicineId { get; set; }

    public int BatchId { get; set; }

    public StockLocation Location { get; set; }

    public decimal Quantity { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Medicine Medicine { get; set; } = null!;

    public Batch Batch { get; set; } = null!;
}