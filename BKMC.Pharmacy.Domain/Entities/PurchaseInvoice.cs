using System.Text.RegularExpressions;

namespace BKMC.Pharmacy.Domain.Entities;

public class PurchaseInvoice                                                               //Receiving//
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTime InvoiceDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Supplier Supplier { get; set; } = null!;

    public ICollection<PurchaseItem> PurchaseItems { get; set; }
        = new List<PurchaseItem>();
}

public class PurchaseItem
{
    public int Id { get; set; }

    public int PurchaseInvoiceId { get; set; }

    public int MedicineId { get; set; }

    public int BatchId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public PurchaseInvoice PurchaseInvoice { get; set; } = null!;

    public Medicine Medicine { get; set; } = null!;

    public Batch Batch { get; set; } = null!;
}