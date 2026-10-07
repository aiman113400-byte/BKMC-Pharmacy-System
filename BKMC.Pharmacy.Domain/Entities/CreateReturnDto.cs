namespace BKMC.Pharmacy.Application.DTOs.Return;

public class CreateReturnDto
{
    public int PatientId { get; set; }

    public int? DispensingId { get; set; }

    public DateTime ReturnDate { get; set; }

    public string? Reason { get; set; }

    public List<CreateReturnItemDto> Items { get; set; }
        = new();
}

public class CreateReturnItemDto
{
    public int DispensingItemId { get; set; }

    public decimal Quantity { get; set; }
}

public class ReturnDto
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string MRNumber { get; set; } = string.Empty;

    public string PatientName { get; set; } = string.Empty;

    public int? DispensingId { get; set; }

    public DateTime ReturnDate { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<ReturnItemDto> Items { get; set; }
        = new();
}

public class ReturnItemDto
{
    public int Id { get; set; }

    public int DispensingItemId { get; set; }

    public int MedicineId { get; set; }

    public string MedicineName { get; set; } = string.Empty;

    public int BatchId { get; set; }

    public string BatchNumber { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
}