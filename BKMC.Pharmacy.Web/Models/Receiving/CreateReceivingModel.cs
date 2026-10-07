using System.ComponentModel.DataAnnotations;

namespace BKMC.Pharmacy.Web.Models.Receiving;

public class CreateReceivingModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a supplier.")]
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "Invoice number is required.")]
    [StringLength(
        100,
        ErrorMessage = "Invoice number cannot exceed 100 characters.")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Invoice date is required.")]
    public DateTime InvoiceDate { get; set; }

    public List<ReceivingItemModel> Items { get; set; } = new();
}

public class ReceivingItemModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a medicine.")]
    public int MedicineId { get; set; }

    [Required(ErrorMessage = "Batch number is required.")]
    [StringLength(
        100,
        ErrorMessage = "Batch number cannot exceed 100 characters.")]
    public string BatchNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Manufacturing date is required.")]
    public DateTime ManufacturingDate { get; set; }

    [Required(ErrorMessage = "Expiry date is required.")]
    public DateTime ExpiryDate { get; set; }

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