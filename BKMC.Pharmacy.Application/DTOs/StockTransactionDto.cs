using BKMC.Pharmacy.Domain.Enums;

namespace BKMC.Pharmacy.Application.DTOs.StockTransactions;

public class StockTransactionDto                                             //Database se response dene ke liye.//
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
}

public class CreateStockTransactionDto                                              //New transaction create karne ke liye.//
{
    public int MedicineId { get; set; }

    public int BatchId { get; set; }

    public StockLocation Location { get; set; }

    public StockTransactionType TransactionType { get; set; }

    public decimal Quantity { get; set; }

    public decimal BalanceQuantity { get; set; }

    public string? ReferenceNumber { get; set; }

    public DateTime TransactionDate { get; set; }
}