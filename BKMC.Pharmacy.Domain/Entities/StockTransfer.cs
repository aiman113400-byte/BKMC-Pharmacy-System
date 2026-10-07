using BKMC.Pharmacy.Domain.Enums;

namespace BKMC.Pharmacy.Domain.Entities;

public class StockTransfer
{
    public int Id { get; set; }

    public StockLocation FromLocation { get; set; }

    public StockLocation ToLocation { get; set; }

    public DateTime TransferDate { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<StockTransferItem> Items { get; set; }
        = new List<StockTransferItem>();
}