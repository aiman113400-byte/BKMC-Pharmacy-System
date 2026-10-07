using System.ComponentModel.DataAnnotations;
using BKMC.Pharmacy.Domain.Enums;


namespace BKMC.Pharmacy.Web.Models.StockAdjustments;

public class CreateStockAdjustmentModel
{
    [Required]
    public int MedicineId { get; set; }

    [Required]
    public int BatchId { get; set; }

    [Required]
    public StockLocation Location { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "New quantity cannot be negative.")]
    public decimal NewQuantity { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    [Required]
    public DateTime AdjustmentDate { get; set; } = DateTime.Now;
}

public class StockAdjustmentDto
{
    public int Id { get; set; }

    public int MedicineId { get; set; }

    public int BatchId { get; set; }

    public StockLocation Location { get; set; }

    public decimal PreviousQuantity { get; set; }

    public decimal NewQuantity { get; set; }

    public decimal Difference { get; set; }

    public string? Reason { get; set; }

    public DateTime AdjustmentDate { get; set; }

    public DateTime CreatedAt { get; set; }
}