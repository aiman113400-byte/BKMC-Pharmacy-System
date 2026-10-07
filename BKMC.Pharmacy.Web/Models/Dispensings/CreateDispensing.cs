using System.ComponentModel.DataAnnotations;

namespace BKMC.Pharmacy.Web.Models.Dispensings;

public class CreateDispensingItemModel
{
    [Required]
    public int MedicineId { get; set; }

    public int? BatchId { get; set; }

    [Range(
        0.01,
        double.MaxValue,
        ErrorMessage = "Quantity must be greater than zero.")]
    public decimal Quantity { get; set; }

    public string? Dosage { get; set; }
}

public class CreateDispensingModel
{
    [Required]
    public int PatientId { get; set; }

    [Required]
    public DateTime DispensingDate { get; set; }

    public string? PrescriptionNumber { get; set; }

    public string? Remarks { get; set; }

    public List<CreateDispensingItemModel> Items { get; set; }
        = new();
}