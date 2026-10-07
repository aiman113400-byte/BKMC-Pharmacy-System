using System.ComponentModel.DataAnnotations;

namespace BKMC.Pharmacy.Web.Models.Medicines;

public class MedicineDto
{
    public int Id { get; set; }

    public string MedicineCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string GenericName { get; set; } = string.Empty;

    public string Strength { get; set; } = string.Empty;

    public string Form { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class CreateMedicineModel
{
    [Required]
    [MaxLength(50)]
    public string MedicineCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string GenericName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Strength { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Form { get; set; } = string.Empty;
}

public class UpdateMedicineModel
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string GenericName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Strength { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Form { get; set; } = string.Empty;
}