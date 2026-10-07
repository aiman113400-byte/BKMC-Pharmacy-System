namespace BKMC.Pharmacy.Domain.Enums;

public enum StockTransactionType
{
    Receiving = 1,
    TransferOut = 2,
    TransferIn = 3,
    Dispensing = 4,
    Return = 5,
    Adjustment = 6
}