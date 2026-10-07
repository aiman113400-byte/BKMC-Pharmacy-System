namespace BKMC.Pharmacy.Domain.Entities;

public class Dispensing
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public DateTime DispensingDate { get; set; }

    public string? PrescriptionNumber { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient Patient { get; set; } = null!;

    public ICollection<DispensingItem> Items { get; set; }
        = new List<DispensingItem>();
}

public class DispensingItem
{
    public int Id { get; set; }

    public int DispensingId { get; set; }

    public int MedicineId { get; set; }

    public int BatchId { get; set; }

    public decimal Quantity { get; set; }

    public string? Dosage { get; set; }

    public Dispensing Dispensing { get; set; } = null!;

    public Medicine Medicine { get; set; } = null!;

    public Batch Batch { get; set; } = null!;
}