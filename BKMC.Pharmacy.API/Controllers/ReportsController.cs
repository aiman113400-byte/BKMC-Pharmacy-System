
using BKMC.Pharmacy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BKMC.Pharmacy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("stock-summary")]
    public async Task<IActionResult> GetStockSummary()
    {
        var report = await _reportService.GetStockSummaryAsync();

        return Ok(report);
    }
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var report = await _reportService.GetLowStockReportAsync();

        return Ok(report);
    }

    [HttpGet("daily-sales")]
    public async Task<IActionResult> GetDailySales([FromQuery] DateTime? date)
    {
        var reportDate = date ?? DateTime.Today;

        var report =
            await _reportService.GetDailySalesAsync(reportDate);

        return Ok(report);
    }

    [HttpGet("monthly-sales")]
    public async Task<IActionResult> GetMonthlySales(
    [FromQuery] int? year,
    [FromQuery] int? month)
    {
        var reportYear = year ?? DateTime.Today.Year;
        var reportMonth = month ?? DateTime.Today.Month;

        if (reportMonth < 1 || reportMonth > 12)
        {
            return BadRequest("Month must be between 1 and 12.");
        }

        var report =
            await _reportService.GetMonthlySalesAsync(
                reportYear,
                reportMonth);

        return Ok(report);
    }
}
