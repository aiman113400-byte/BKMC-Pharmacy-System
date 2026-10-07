namespace BKMC.Pharmacy.Application.DTOs.Dashboard;

public class DashboardDto
{
    public int TotalMedicines { get; set; }

    public int ActiveMedicines { get; set; }

    public int TotalStock { get; set; }

    public int LowStock { get; set; }

    public int OutOfStock { get; set; }

    public int ExpiringSoon { get; set; }

    public int TodayReceiving { get; set; }

    public int TodayDispensing { get; set; }

    public decimal TodayBilling { get; set; }

    public int PendingReturns { get; set; }
}