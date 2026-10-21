
using BKMC.Pharmacy.Application.DTOs.Reports;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Enums;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Application.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StockSummaryReportDto> GetStockSummaryAsync()
    {
        var stocks = await _context.Stocks
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.MedicineId,
                MedicineName = x.Medicine.Name,
                x.BatchId,
                BatchNumber = x.Batch.BatchNumber,
                x.Location,
                x.Quantity,
                x.UpdatedAt
            })
            .ToListAsync();

        var stockItems = stocks.Select(x => new
        BKMC.Pharmacy.Application.DTOs.Stock.StockDto
        {
            Id = x.Id,
            MedicineId = x.MedicineId,
            MedicineName = x.MedicineName,
            BatchId = x.BatchId,
            BatchNumber = x.BatchNumber,
            Location = x.Location,
            Quantity = x.Quantity,
            UpdatedAt = x.UpdatedAt
        }).ToList();

        return new StockSummaryReportDto
        {
            TotalStockRecords = stocks.Count,

            TotalWarehouseQuantity = stocks
                .Where(x => x.Location == StockLocation.Warehouse)
                .Sum(x => x.Quantity),

            TotalPharmacyQuantity = stocks
                .Where(x => x.Location == StockLocation.Pharmacy)
                .Sum(x => x.Quantity),

            TotalQuantity = stocks.Sum(x => x.Quantity),

            StockItems = stockItems
        };
    }

    public async Task<List<LowStockReportDto>> GetLowStockReportAsync()
    {
        var report = await _context.Medicines
            .AsNoTracking()
            .Where(m => m.IsActive && m.ReorderLevel > 0)
            .Select(m => new LowStockReportDto
            {
                MedicineId = m.Id,
                MedicineCode = m.MedicineCode,
                MedicineName = m.Name,
                ReorderLevel = m.ReorderLevel,

                WarehouseQuantity = _context.Stocks
                    .Where(s =>
                        s.MedicineId == m.Id &&
                        s.Location == StockLocation.Warehouse)
                    .Sum(s => (decimal?)s.Quantity) ?? 0m,

                PharmacyQuantity = _context.Stocks
                    .Where(s =>
                        s.MedicineId == m.Id &&
                        s.Location == StockLocation.Pharmacy)
                    .Sum(s => (decimal?)s.Quantity) ?? 0m
            })
            .ToListAsync();

        foreach (var item in report)
        {
            item.TotalQuantity =
                item.WarehouseQuantity +
                item.PharmacyQuantity;
        }

        return report
            .Where(x => x.TotalQuantity < x.ReorderLevel)
            .ToList();
    }

    public async Task<DailySalesReportDto> GetDailySalesAsync(DateTime date)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        var bills = await _context.Bills
            .AsNoTracking()
            .Where(b =>
                b.BillDate >= startDate &&
                b.BillDate < endDate)
            .Select(b => new DailySalesBillDto
            {
                BillId = b.Id,
                BillNumber = b.BillNumber,

                PatientId = b.PatientId,
                PatientName = b.Patient.Name,

                BillDate = b.BillDate,

                TotalAmount = b.TotalAmount,
                PaidAmount = b.PaidAmount,
                BalanceAmount = b.BalanceAmount,

                MedicinesSold = b.Items
                    .Sum(i => (decimal?)i.Quantity) ?? 0m
            })
            .OrderByDescending(b => b.BillDate)
            .ToListAsync();

        return new DailySalesReportDto
        {
            Date = startDate,

            TotalBills = bills.Count,

            GrossSales = bills.Sum(b => b.TotalAmount),

            PaidAmount = bills.Sum(b => b.PaidAmount),

            BalanceAmount = bills.Sum(b => b.BalanceAmount),

            MedicinesSold = bills.Sum(b => b.MedicinesSold),

            Bills = bills
        };
    }

    public async Task<MonthlySalesReportDto> GetMonthlySalesAsync(
    int year,
    int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var bills = await _context.Bills
            .AsNoTracking()
            .Where(b =>
                b.BillDate >= startDate &&
                b.BillDate < endDate)
            .Select(b => new
            {
                b.BillDate,
                b.TotalAmount,
                b.PaidAmount,
                b.BalanceAmount,

                MedicinesSold = b.Items
                    .Sum(i => (decimal?)i.Quantity) ?? 0m
            })
            .ToListAsync();

        var dailySales = bills
            .GroupBy(b => b.BillDate.Date)
            .Select(g => new MonthlySalesDayDto
            {
                Date = g.Key,

                TotalBills = g.Count(),

                GrossSales = g.Sum(x => x.TotalAmount),

                PaidAmount = g.Sum(x => x.PaidAmount),

                BalanceAmount = g.Sum(x => x.BalanceAmount),

                MedicinesSold = g.Sum(x => x.MedicinesSold)
            })
            .OrderByDescending(x => x.Date)
            .ToList();

        return new MonthlySalesReportDto
        {
            Year = year,

            Month = month,

            MonthName = startDate.ToString("MMMM"),

            TotalBills = bills.Count,

            GrossSales = bills.Sum(x => x.TotalAmount),

            PaidAmount = bills.Sum(x => x.PaidAmount),

            BalanceAmount = bills.Sum(x => x.BalanceAmount),

            MedicinesSold = bills.Sum(x => x.MedicinesSold),

            DailySales = dailySales
        };
    }
}
