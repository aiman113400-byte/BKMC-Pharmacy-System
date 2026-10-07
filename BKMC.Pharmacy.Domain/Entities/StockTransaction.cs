using BKMC.Pharmacy.Domain.Enums;

namespace BKMC.Pharmacy.Domain.Entities;

public class StockTransaction
{
    public int Id { get; set; }

    public int MedicineId { get; set; }

    public int BatchId { get; set; }

    public StockLocation Location { get; set; }

    public StockTransactionType TransactionType { get; set; }

    public decimal Quantity { get; set; }

    public decimal BalanceQuantity { get; set; }

    public string? ReferenceNumber { get; set; }

    public DateTime TransactionDate { get; set; }

    public Medicine Medicine { get; set; } = null!;

    public Batch Batch { get; set; } = null!;
}