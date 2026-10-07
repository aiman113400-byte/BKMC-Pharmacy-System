using System.ComponentModel.DataAnnotations;

namespace BKMC.Pharmacy.Web.Models.Billing;

public class CreateBillItemModel
{
    [Required]
    public int MedicineId { get; set; }

    [Range(
        0.01,
        double.MaxValue,
        ErrorMessage = "Quantity must be greater than zero.")]
    public decimal Quantity { get; set; }

    [Range(
        0,
        double.MaxValue,
        ErrorMessage = "Unit price cannot be negative.")]
    public decimal UnitPrice { get; set; }
}

public class CreateBillModel
{
    [Required]
    public int PatientId { get; set; }

    public int? DispensingId { get; set; }

    [Required]
    public DateTime BillDate { get; set; }

    [Range(
        0,
        double.MaxValue,
        ErrorMessage = "Paid amount cannot be negative.")]
    public decimal PaidAmount { get; set; }

    public List<CreateBillItemModel> Items { get; set; }
        = new();
}