using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;


public class CreateMedicineDto
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

    [Range(0, double.MaxValue, ErrorMessage = "Reorder level cannot be negative.")]
    public decimal ReorderLevel { get; set; } = 0;
}
