
namespace BKMC.Pharmacy.Domain.Entities;

public class BackupHistory
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Success or Failed
    public string Status { get; set; } = "Success";

    public string? CreatedBy { get; set; }

    public string? ErrorMessage { get; set; }
}

public class BackupHistoryDto
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? CreatedBy { get; set; }

    public string? ErrorMessage { get; set; }
}
