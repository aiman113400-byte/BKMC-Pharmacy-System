using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Reports;

public class IndexModel : PageModel
{
    private readonly ReportsApiService _reportsApiService;

    public StockSummaryReportDto StockSummary { get; private set; } = new();

    public List<LowStockReportDto> LowStockItems { get; private set; } = new();

    public DailySalesReportDto DailySales { get; private set; } = new();

    public MonthlySalesReportDto MonthlySales { get; private set; } = new();

    public int SelectedYear { get; private set; }

    public int SelectedMonth { get; private set; }

    public bool ApiAvailable { get; private set; } = true;

    public string ApiMessage { get; private set; } = string.Empty;

    public DateTime SelectedDate { get; private set; }

    public IndexModel(ReportsApiService reportsApiService)
    {
        _reportsApiService = reportsApiService;
    }

    public async Task OnGetAsync(
        DateTime? date,
        int? year,
        int? month)
    {
        try
        {
            SelectedDate = date?.Date ?? DateTime.Today;

            StockSummary =
                await _reportsApiService.GetStockSummaryAsync()
                ?? new StockSummaryReportDto();

            LowStockItems =
                await _reportsApiService.GetLowStockReportAsync();

            DailySales =
                await _reportsApiService.GetDailySalesAsync(SelectedDate)
                ?? new DailySalesReportDto
                {
                    Date = SelectedDate
                };

            SelectedYear = year ?? DateTime.Today.Year;
            SelectedMonth = month ?? DateTime.Today.Month;

            MonthlySales =
                await _reportsApiService.GetMonthlySalesAsync(
                    SelectedYear,
                    SelectedMonth)
                ?? new MonthlySalesReportDto
                {
                    Year = SelectedYear,
                    Month = SelectedMonth, // <--- Yahan comma add kiya gaya hai
                    MonthName = new DateTime(SelectedYear, SelectedMonth, 1).ToString("MMMM")
                };
        }
        catch
        {
            ApiAvailable = false;

            ApiMessage =
                "Unable to load reports. Please make sure the API is running.";
        }
    }
}



//using BKMC.Pharmacy.Web.Services;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//namespace BKMC.Pharmacy.Web.Pages.Reports;

//public class IndexModel : PageModel
//{
//    private readonly ReportsApiService _reportsApiService;

//    public StockSummaryReportDto StockSummary { get; private set; } = new();

//    public List<LowStockReportDto> LowStockItems { get; private set; } = new();

//    public DailySalesReportDto DailySales { get; private set; } = new();

//    public MonthlySalesReportDto MonthlySales { get; private set; } = new();

//    public int SelectedYear { get; private set; }

//    public int SelectedMonth { get; private set; }

//    public bool ApiAvailable { get; private set; } = true;

//    public string ApiMessage { get; private set; } = string.Empty;

//    public DateTime SelectedDate { get; private set; }

//    public IndexModel(ReportsApiService reportsApiService)
//    {
//        _reportsApiService = reportsApiService;
//    }
//    public async Task OnGetAsync(
//        DateTime? date,
//        int? year,
//        int? month)
//    {
//        try
//        {
//            SelectedDate = date?.Date ?? DateTime.Today;

//            StockSummary =
//                await _reportsApiService.GetStockSummaryAsync()
//                ?? new StockSummaryReportDto();

//            LowStockItems =
//                await _reportsApiService.GetLowStockReportAsync();

//            DailySales =
//                await _reportsApiService.GetDailySalesAsync(SelectedDate)
//                ?? new DailySalesReportDto
//                {
//                    Date = SelectedDate
//                };
//            SelectedYear = year ?? DateTime.Today.Year;
//            SelectedMonth = month ?? DateTime.Today.Month;

//            MonthlySales =
//                await _reportsApiService.GetMonthlySalesAsync(
//                    SelectedYear,
//                    SelectedMonth)
//                ?? new MonthlySalesReportDto
//                {
//                    Year = SelectedYear,
//                    Month = SelectedMonth
//                     MonthName = new DateTime(SelectedYear, SelectedMonth, 1).ToString("MMMM")
//                };
//        }
//        catch
//        {
//            ApiAvailable = false;

//            ApiMessage =
//                "Unable to load reports. Please make sure the API is running.";
//        }
//    }

    //public async Task OnGetAsync(DateTime? date)
    //{
    //    try
    //    {
    //        StockSummary =
    //            await _reportsApiService.GetStockSummaryAsync()
    //            ?? new StockSummaryReportDto();

    //        LowStockItems =
    //            await _reportsApiService.GetLowStockReportAsync();
    //    }
    //    catch
    //    {
    //        ApiAvailable = false;

    //        ApiMessage =
    //            "Unable to load reports. Please make sure the API is running.";
    //    }
    //}
//}