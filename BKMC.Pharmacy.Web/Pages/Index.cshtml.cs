
using BKMC.Pharmacy.Web.Models.Dashboard;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages;

public class IndexModel : PageModel
{
    private readonly DashboardApiService _dashboardApiService;

    public DashboardViewModel Dashboard { get; private set; } = new();

    public IndexModel(DashboardApiService dashboardApiService)
    {
        _dashboardApiService = dashboardApiService;
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

        Dashboard = await _dashboardApiService.GetDashboardAsync();

        return Page();
    }
}





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