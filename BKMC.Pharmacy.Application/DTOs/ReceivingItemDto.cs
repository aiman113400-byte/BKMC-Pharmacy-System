namespace BKMC.Pharmacy.Application.DTOs.Receiving;

public class ReceivingItemDto
{
    public int MedicineId { get; set; }

    public string BatchNumber { get; set; } = string.Empty;

    public DateTime ManufacturingDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}

public class CreateReceivingDto
{
    public int SupplierId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTime InvoiceDate { get; set; }

    public List<ReceivingItemDto> Items { get; set; } = new();
}

public class ReceivingDto
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public string SupplierName { get; set; } = string.Empty;

    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTime InvoiceDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<ReceivingItemDto> Items { get; set; } = new();
}