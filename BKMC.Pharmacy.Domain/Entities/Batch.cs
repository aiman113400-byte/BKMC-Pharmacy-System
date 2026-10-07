using System.Collections;

namespace BKMC.Pharmacy.Domain.Entities;

public class Batch
{
    public int Id { get; set; }

    public int MedicineId { get; set; }

    public string BatchNumber { get; set; } = string.Empty;

    public DateTime ManufacturingDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public decimal Quantity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Medicine Medicine { get; set; } = null!;

    public ICollection<PurchaseItem> PurchaseItems { get; set; }
        = new List<PurchaseItem>();

    public ICollection<Stock> Stocks { get; set; }
        = new List<Stock>();
}