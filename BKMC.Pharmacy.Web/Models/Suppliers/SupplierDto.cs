using System.ComponentModel.DataAnnotations;

namespace BKMC.Pharmacy.Web.Models.Suppliers;

public class SupplierDto
{
    public int Id { get; set; }

    public string SupplierCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class CreateSupplierModel
{
    [Required(ErrorMessage = "Supplier code is required.")]
    [StringLength(50)]
    public string SupplierCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Supplier name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ContactPerson { get; set; }

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }
}

public class UpdateSupplierModel
{
    [Required(ErrorMessage = "Supplier code is required.")]
    [StringLength(50)]
    public string SupplierCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Supplier name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ContactPerson { get; set; }

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    public bool IsActive { get; set; }
}