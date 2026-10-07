using System.Net.Http.Json;
using BKMC.Pharmacy.Web.Models.Dashboard;

namespace BKMC.Pharmacy.Web.Services;

public class DashboardApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DashboardApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var model = new DashboardViewModel();

        try
        {
            var client = _httpClientFactory.CreateClient("BKMC.Pharmacy.API");

            var dashboard = await client.GetFromJsonAsync<DashboardApiResponse>(
                "api/Dashboard");

            if (dashboard == null)
            {
                model.ApiAvailable = false;
                model.ApiMessage = "Dashboard data was not returned by the API.";
                return model;
            }

            model.TotalMedicines = CreateMetric(
                dashboard.TotalMedicines.ToString(),
                "Total Medicines",
                "bi-capsule");

            model.ActiveMedicines = CreateMetric(
                dashboard.ActiveMedicines.ToString(),
                "Active Medicines",
                "bi-check-circle");

            model.TotalStock = CreateMetric(
                dashboard.TotalStock.ToString(),
                "Total Stock",
                "bi-box-seam");

            model.LowStock = CreateMetric(
                dashboard.LowStock.ToString(),
                "Low Stock",
                "bi-exclamation-triangle");

            model.OutOfStock = CreateMetric(
                dashboard.OutOfStock.ToString(),
                "Out of Stock",
                "bi-x-circle");

            model.ExpiringSoon = CreateMetric(
                dashboard.ExpiringSoon.ToString(),
                "Expiring Soon",
                "bi-calendar-x");

            model.TodayReceiving = CreateMetric(
                dashboard.TodayReceiving.ToString(),
                "Today's Receiving",
                "bi-box-arrow-in-down");

            model.TodayDispensing = CreateMetric(
                dashboard.TodayDispensing.ToString(),
                "Today's Dispensing",
                "bi-prescription2");

            model.TodayBilling = CreateMetric(
                dashboard.TodayBilling.ToString("N2"),
                "Today's Billing",
                "bi-receipt");

            model.PendingReturns = CreateMetric(
                dashboard.PendingReturns.ToString(),
                "Pending Returns",
                "bi-arrow-return-left");

            model.ApiAvailable = true;
            model.ApiMessage = "Dashboard data loaded successfully.";

            return model;
        }
        //catch (HttpRequestException)
        //{
        //    model.ApiAvailable = false;
        //    model.ApiMessage = "Unable to connect to the BKMC Pharmacy API.";
        //    return model;
        //}
        catch (HttpRequestException ex)
        {
            model.ApiAvailable = false;
            model.ApiMessage = $"API connection failed: {ex.Message}";
            return model;
        }
        catch (Exception)
        {
            model.ApiAvailable = false;
            model.ApiMessage = "An unexpected error occurred while loading dashboard data.";
            return model;
        }
    }

    private static DashboardMetric CreateMetric(
        string value,
        string label,
        string icon)
    {
        return new DashboardMetric
        {
            Value = value,
            Label = label,
            Icon = icon,
            Status = "Available"
        };
    }
}

public class DashboardApiResponse
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