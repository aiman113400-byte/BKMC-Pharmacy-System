using System.ComponentModel.DataAnnotations;

namespace BKMC.Pharmacy.Web.Models.Patients;

public class PatientDto
{
    public int Id { get; set; }

    public string MRNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }

    public Gender Gender { get; set; }

    public string? AdmissionNumber { get; set; }

    public string? ReferralId { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreatePatientModel
{
    [Required(ErrorMessage = "MR Number is required.")]
    public string MRNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Patient name is required.")]
    public string Name { get; set; } = string.Empty;

    [Range(
        0,
        150,
        ErrorMessage = "Age must be between 0 and 150.")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Gender is required.")]
    public int  Gender { get; set; } 

    public string? AdmissionNumber { get; set; }

    public string? ReferralId { get; set; }
}

public class UpdatePatientModel
{
    [Required(ErrorMessage = "MR Number is required.")]
    public string MRNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Patient name is required.")]
    public string Name { get; set; } = string.Empty;

    [Range(
        0,
        150,
        ErrorMessage = "Age must be between 0 and 150.")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Gender is required.")]
    public Gender Gender { get; set; }

    public string? AdmissionNumber { get; set; }

    public string? ReferralId { get; set; }
}