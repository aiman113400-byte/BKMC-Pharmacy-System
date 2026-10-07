using BKMC.Pharmacy.Web.Models.Stock;

namespace BKMC.Pharmacy.Web.Models.StockTransfers;

public class CreateStockTransferDto
{
    public StockLocation FromLocation { get; set; }

    public StockLocation ToLocation { get; set; }

    public DateTime TransferDate { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;

    public string? Remarks { get; set; }

    public List<CreateStockTransferItemDto> Items { get; set; }
        = new();
}