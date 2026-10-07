using System.ComponentModel.DataAnnotations;

namespace BKMC.Pharmacy.Web.Models.Returns;

public class CreateReturnItemModel
{
    [Required]
    public int DispensingItemId { get; set; }

    [Range(
        0.01,
        double.MaxValue,
        ErrorMessage = "Return quantity must be greater than zero.")]
    public decimal Quantity { get; set; }
}

public class CreateReturnModel
{
    [Required]
    public int PatientId { get; set; }

    public int? DispensingId { get; set; }

    [Required]
    public DateTime ReturnDate { get; set; }

    public string? Reason { get; set; }

    public List<CreateReturnItemModel> Items { get; set; }
        = new();
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

    public List<ReturnItemDto> Items { get; set; } = new();
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