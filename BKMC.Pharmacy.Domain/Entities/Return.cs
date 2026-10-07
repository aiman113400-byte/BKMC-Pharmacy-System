namespace BKMC.Pharmacy.Domain.Entities;

public class Return
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int? DispensingId { get; set; }

    public DateTime ReturnDate { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient Patient { get; set; } = null!;

    public Dispensing? Dispensing { get; set; }

    public ICollection<ReturnItem> Items { get; set; }
        = new List<ReturnItem>();
}

public class ReturnItem
{
    public int Id { get; set; }

    public int ReturnId { get; set; }

    public int MedicineId { get; set; }

    public int BatchId { get; set; }

    public decimal Quantity { get; set; }

    public Return Return { get; set; } = null!;

    public Medicine Medicine { get; set; } = null!;

    public Batch Batch { get; set; } = null!;

    public int DispensingItemId { get; set; }                                                //add new column//

    public DispensingItem DispensingItem { get; set; } = null!;                               //add new column//
}