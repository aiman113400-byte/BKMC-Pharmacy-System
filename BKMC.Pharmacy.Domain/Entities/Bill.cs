namespace BKMC.Pharmacy.Domain.Entities;

public class Bill
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int? DispensingId { get; set; }

    public string BillNumber { get; set; } = string.Empty;

    public DateTime BillDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient Patient { get; set; } = null!;

    public Dispensing? Dispensing { get; set; }

    public ICollection<BillItem> Items { get; set; }
        = new List<BillItem>();
}

public class BillItem
{
    public int Id { get; set; }

    public int BillId { get; set; }

    public int MedicineId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public Bill Bill { get; set; } = null!;

    public Medicine Medicine { get; set; } = null!;
}