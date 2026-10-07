using BKMC.Pharmacy.Application.DTOs.Dashboard;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var expiryLimit = today.AddDays(30);

        // =========================================
        // MEDICINE METRICS
        // =========================================

        var totalMedicines = await _context.Medicines
            .AsNoTracking()
            .CountAsync();

        var activeMedicines = await _context.Medicines
            .AsNoTracking()
            .CountAsync(x => x.IsActive);


        // =========================================
        // STOCK METRICS
        // =========================================

        var totalStock = await _context.Stocks
            .AsNoTracking()
            .SumAsync(x => x.Quantity);

        var lowStock = await _context.Stocks
            .AsNoTracking()
            .CountAsync(x =>
                x.Quantity > 0 &&
                x.Quantity <= 5);

        var outOfStock = await _context.Stocks
            .AsNoTracking()
            .CountAsync(x =>
                x.Quantity <= 0);


        // =========================================
        // EXPIRY METRICS
        // =========================================

        var expiringSoon = await _context.Stocks
            .AsNoTracking()
            .Include(x => x.Batch)
            .CountAsync(x =>
                x.Quantity > 0 &&
                x.Batch.ExpiryDate.Date > today &&
                x.Batch.ExpiryDate.Date <= expiryLimit);


        // =========================================
        // TODAY'S RECEIVING
        // =========================================

        var todayReceiving = await _context.PurchaseInvoices
            .AsNoTracking()
            .CountAsync(x =>
                x.InvoiceDate >= today &&
                x.InvoiceDate < tomorrow);


        // =========================================
        // TODAY'S DISPENSING
        // =========================================

        var todayDispensing = await _context.Dispensings
            .AsNoTracking()
            .CountAsync(x =>
                x.DispensingDate >= today &&
                x.DispensingDate < tomorrow);


        // =========================================
        // TODAY'S BILLING
        // =========================================

        var todayBilling = await _context.Bills
            .AsNoTracking()
            .Where(x =>
                x.BillDate >= today &&
                x.BillDate < tomorrow)
            .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;


        // =========================================
        // RETURNS
        // =========================================
        //
        // Current Return entity does not contain
        // Status/Pending property.
        //
        // Therefore we do NOT label a count as
        // "Pending Returns" without a business rule.
        //
        // Temporary value remains unavailable.
        // =========================================

        return new DashboardDto
        {
            TotalMedicines = totalMedicines,

            ActiveMedicines = activeMedicines,

            TotalStock = Convert.ToInt32(totalStock),

            LowStock = lowStock,

            OutOfStock = outOfStock,

            ExpiringSoon = expiringSoon,

            TodayReceiving = todayReceiving,

            TodayDispensing = todayDispensing,

            TodayBilling = todayBilling,

            PendingReturns = 0
        };
    }
}