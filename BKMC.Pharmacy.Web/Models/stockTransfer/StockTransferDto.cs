using BKMC.Pharmacy.Web.Models.Stock;

namespace BKMC.Pharmacy.Web.Models.StockTransfers;

public class StockTransferDto
{
    public int Id { get; set; }

    public StockLocation FromLocation { get; set; }

    public StockLocation ToLocation { get; set; }

    public DateTime TransferDate { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<StockTransferItemDto> Items { get; set; }
        = new();
}

public class StockTransferItemDto
{
    public int MedicineId { get; set; }

    public string MedicineName { get; set; } = string.Empty;

    public int BatchId { get; set; }

    public string BatchNumber { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
}