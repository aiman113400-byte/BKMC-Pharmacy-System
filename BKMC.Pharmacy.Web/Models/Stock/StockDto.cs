


namespace BKMC.Pharmacy.Web.Models.Stock;

public class StockDto
{
    public int Id { get; set; }

    public int MedicineId { get; set; }

    public string MedicineName { get; set; } = string.Empty;

    public int BatchId { get; set; }

    public string BatchNumber { get; set; } = string.Empty;

    public StockLocation Location { get; set; }

    public decimal Quantity { get; set; }

    public DateTime UpdatedAt { get; set; }
}