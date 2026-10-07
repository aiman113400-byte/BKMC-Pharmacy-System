using BKMC.Pharmacy.Domain.Enums;

namespace BKMC.Pharmacy.Application.DTOs.Patients;

public class CreatePatientDto
{
    public string MRNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }

    public Gender Gender { get; set; }

    public string? AdmissionNumber { get; set; }

    public string? ReferralId { get; set; }
}


public class UpdatePatientDto
{
    public string MRNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }

    public Gender Gender { get; set; }

    public string? AdmissionNumber { get; set; }

    public string? ReferralId { get; set; }
}

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

public class PatientLookupDto
{
    public int Id { get; set; }

    public string MRNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string? AdmissionNumber { get; set; }

    public string? ReferralId { get; set; }
}