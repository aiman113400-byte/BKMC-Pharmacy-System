namespace BKMC.Pharmacy.Web.Models.Batches;

public class BatchDto
{
    public int Id { get; set; }

    public int MedicineId { get; set; }

    public string MedicineName { get; set; } = string.Empty;

    public string BatchNumber { get; set; } = string.Empty;

    public DateTime ManufacturingDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public decimal Quantity { get; set; }

    public DateTime CreatedAt { get; set; }
}


public class BatchLookupDto
{
    public int Id { get; set; }

    public int MedicineId { get; set; }

    public string BatchNumber { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }

    public decimal AvailableQuantity { get; set; }

    public bool IsExpired { get; set; }
}