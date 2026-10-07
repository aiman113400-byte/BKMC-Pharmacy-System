using BKMC.Pharmacy.Domain.Enums;
using System.Reflection;

namespace BKMC.Pharmacy.Domain.Entities;

public class Patient
{
    public int Id { get; set; }

    public string MRNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }

    public Gender Gender { get; set; }

    public string? AdmissionNumber { get; set; }

    public string? ReferralId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Dispensing> Dispensings { get; set; }
        = new List<Dispensing>();

    public ICollection<Bill> Bills { get; set; }
        = new List<Bill>();
}