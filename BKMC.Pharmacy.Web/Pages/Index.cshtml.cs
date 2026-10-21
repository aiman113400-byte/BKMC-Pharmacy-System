using BKMC.Pharmacy.Web.Models.Dashboard;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages;

public class IndexModel : PageModel
{
    private readonly DashboardApiService _dashboardApiService;
    private readonly ReportsApiService _reportsApiService;

    public DashboardViewModel Dashboard { get; private set; } = new();

    // ===================== REPORT DATA =====================

    public StockSummaryReportDto StockSummary { get; private set; } = new();

    public List<LowStockReportDto> LowStockItems { get; private set; } = new();

    public DailySalesReportDto DailySales { get; private set; } = new();

    public MonthlySalesReportDto MonthlySales { get; private set; } = new();

    public bool ReportsAvailable { get; private set; } = true;

    public string ReportsMessage { get; private set; } = string.Empty;


    public IndexModel(
        DashboardApiService dashboardApiService,
        ReportsApiService reportsApiService)
    {
        _dashboardApiService = dashboardApiService;
        _reportsApiService = reportsApiService;
    }


    public async Task<IActionResult> OnGetAsync()
    {
        var token = HttpContext.Session.GetString("JWToken");

        var role = HttpContext.Session.GetString("Role");

        if (string.IsNullOrWhiteSpace(token) ||
            !string.Equals(
                role,
                "Admin",
                StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage("/Account/Login");
        }


        // ===================== EXISTING DASHBOARD DATA =====================

        Dashboard = await _dashboardApiService.GetDashboardAsync();


        // ===================== REPORT DATA =====================

        try
        {
            var today = DateTime.Today;

            var stockTask =
                _reportsApiService.GetStockSummaryAsync();

            var lowStockTask =
                _reportsApiService.GetLowStockReportAsync();

            var dailySalesTask =
                _reportsApiService.GetDailySalesAsync(today);

            var monthlySalesTask =
                _reportsApiService.GetMonthlySalesAsync(
                    today.Year,
                    today.Month);


            await Task.WhenAll(
                stockTask,
                lowStockTask,
                dailySalesTask,
                monthlySalesTask);


            StockSummary =
                await stockTask
                ?? new StockSummaryReportDto();


            LowStockItems =
                await lowStockTask
                ?? new List<LowStockReportDto>();


            DailySales =
                await dailySalesTask
                ?? new DailySalesReportDto
                {
                    Date = today
                };


            MonthlySales =
                await monthlySalesTask
                ?? new MonthlySalesReportDto
                {
                    Year = today.Year,
                    Month = today.Month
                };
        }
        catch
        {
            // Existing Dashboard should continue working
            // even if Reports API is temporarily unavailable.

            ReportsAvailable = false;

            ReportsMessage =
                "Sales and inventory analytics are temporarily unavailable.";
        }


        return Page();
    }
}




//using BKMC.Pharmacy.Web.Models.Dashboard;
//using BKMC.Pharmacy.Web.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//namespace BKMC.Pharmacy.Web.Pages;

//public class IndexModel : PageModel
//{
//    private readonly DashboardApiService _dashboardApiService;

//    public DashboardViewModel Dashboard { get; private set; } = new();

//    public IndexModel(DashboardApiService dashboardApiService)
//    {
//        _dashboardApiService = dashboardApiService;
//    }

//    public async Task<IActionResult> OnGetAsync()
//    {
//        var token = HttpContext.Session.GetString("JWToken");

//        var role = HttpContext.Session.GetString("Role");

//        if (string.IsNullOrWhiteSpace(token) ||
//            !string.Equals(
//                role,
//                "Admin",
//                StringComparison.OrdinalIgnoreCase))
//        {
//            return RedirectToPage("/Account/Login");
//        }

//        Dashboard = await _dashboardApiService.GetDashboardAsync();

//        return Page();
//    }
//}





//using BKMC.Pharmacy.Web.Models.Dashboard;
//using BKMC.Pharmacy.Web.Services;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//namespace BKMC.Pharmacy.Web.Pages;

//public class IndexModel : PageModel
//{
//    private readonly DashboardApiService _dashboardApiService;

//    public DashboardViewModel Dashboard { get; private set; } = new();

//    public IndexModel(DashboardApiService dashboardApiService)
//    {
//        _dashboardApiService = dashboardApiService;
//    }

//    public async Task OnGetAsync()
//    {
//        Dashboard = await _dashboardApiService.GetDashboardAsync();
//    }
//}