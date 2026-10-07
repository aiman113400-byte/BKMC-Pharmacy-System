namespace BKMC.Pharmacy.Domain.Entities;

public class StockTransferItem
{
    public int Id { get; set; }

    public int StockTransferId { get; set; }

    public int MedicineId { get; set; }

    public int BatchId { get; set; }

    public decimal Quantity { get; set; }

    public StockTransfer StockTransfer { get; set; } = null!;

    public Medicine Medicine { get; set; } = null!;

    public Batch Batch { get; set; } = null!;
}