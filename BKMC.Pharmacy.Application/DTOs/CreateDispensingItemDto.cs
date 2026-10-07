namespace BKMC.Pharmacy.Application.DTOs.Dispensing;

public class CreateDispensingItemDto
{
    public int MedicineId { get; set; }

    public int? BatchId { get; set; }

    public decimal Quantity { get; set; }

    public string? Dosage { get; set; }
}

public class CreateDispensingDto
{
    public int PatientId { get; set; }

    public DateTime DispensingDate { get; set; }

    public string? PrescriptionNumber { get; set; }

    public string? Remarks { get; set; }

    public List<CreateDispensingItemDto> Items { get; set; }
        = new();
}

public class DispensingItemDto
{
    public int Id { get; set; }                            //add new code here//
    public int MedicineId { get; set; }

    public string MedicineName { get; set; } = string.Empty;

    public int BatchId { get; set; }

    public string BatchNumber { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public string? Dosage { get; set; }
}

public class DispensingDto
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string MRNumber { get; set; } = string.Empty;

    public string PatientName { get; set; } = string.Empty;

    public DateTime DispensingDate { get; set; }

    public string? PrescriptionNumber { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<DispensingItemDto> Items { get; set; }
        = new();
}