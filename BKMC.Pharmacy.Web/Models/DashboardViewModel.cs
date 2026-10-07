namespace BKMC.Pharmacy.Web.Models.Dashboard;

public class DashboardViewModel
{
    public DashboardMetric TotalMedicines { get; set; } = new();

    public DashboardMetric ActiveMedicines { get; set; } = new();

    public DashboardMetric TotalStock { get; set; } = new();

    public DashboardMetric LowStock { get; set; } = new();

    public DashboardMetric OutOfStock { get; set; } = new();

    public DashboardMetric ExpiringSoon { get; set; } = new();

    public DashboardMetric TodayReceiving { get; set; } = new();

    public DashboardMetric TodayDispensing { get; set; } = new();

    public DashboardMetric TodayBilling { get; set; } = new();

    public DashboardMetric PendingReturns { get; set; } = new();

    public bool ApiAvailable { get; set; }

    public string? ApiMessage { get; set; }
}

public class DashboardMetric
{
    public string Value { get; set; } = "—";

    public string Label { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string Status { get; set; } = "Unavailable";
}