namespace BKMC.Pharmacy.Web.Models.StockTransfers;

public class CreateStockTransferItemDto
{
    public int MedicineId { get; set; }

    public int BatchId { get; set; }

    public decimal Quantity { get; set; }
}