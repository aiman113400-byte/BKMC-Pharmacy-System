using BKMC.Pharmacy.Application.DTOs.StockAdjustments;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Domain.Enums;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Infrastructure.Services;

public class StockAdjustmentService : IStockAdjustmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public StockAdjustmentService(
        ApplicationDbContext context,
        IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<IEnumerable<StockAdjustmentDto>> GetAllAsync()
    {
        return await _context.StockAdjustments
            .AsNoTracking()
            .Select(x => new StockAdjustmentDto
            {
                Id = x.Id,
                MedicineId = x.MedicineId,
                BatchId = x.BatchId,
                Location = x.Location,
                PreviousQuantity = x.PreviousQuantity,
                NewQuantity = x.NewQuantity,
                Difference = x.Difference,
                Reason = x.Reason,
                AdjustmentDate = x.AdjustmentDate,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<StockAdjustmentDto?> GetByIdAsync(int id)
    {
        return await _context.StockAdjustments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new StockAdjustmentDto
            {
                Id = x.Id,
                MedicineId = x.MedicineId,
                BatchId = x.BatchId,
                Location = x.Location,
                PreviousQuantity = x.PreviousQuantity,
                NewQuantity = x.NewQuantity,
                Difference = x.Difference,
                Reason = x.Reason,
                AdjustmentDate = x.AdjustmentDate,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<StockAdjustmentDto> CreateAsync(
        CreateStockAdjustmentDto dto)
    {
        // 1. Find existing stock
        var stock = await _context.Stocks
            .FirstOrDefaultAsync(x =>
                x.MedicineId == dto.MedicineId &&
                x.BatchId == dto.BatchId &&
                x.Location == dto.Location);

        if (stock == null)
        {
            throw new InvalidOperationException(
                "Stock record was not found for the specified medicine, batch and location.");
        }

        // 2. Get previous quantity
        var previousQuantity = stock.Quantity;

        // 3. Calculate difference
        var difference = dto.NewQuantity - previousQuantity;

        // 4. Update stock
        stock.Quantity = dto.NewQuantity;
        stock.UpdatedAt = DateTime.UtcNow;

        // 5. Create adjustment record
        var adjustment = new StockAdjustment
        {
            MedicineId = dto.MedicineId,
            BatchId = dto.BatchId,
            Location = dto.Location,
            PreviousQuantity = previousQuantity,
            NewQuantity = dto.NewQuantity,
            Difference = difference,
            Reason = dto.Reason,
            AdjustmentDate = dto.AdjustmentDate == default
                ? DateTime.UtcNow
                : dto.AdjustmentDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.StockAdjustments.Add(adjustment);

        // 6. Create stock transaction
        var transaction = new StockTransaction
        {
            MedicineId = dto.MedicineId,
            BatchId = dto.BatchId,
            Location = dto.Location,
            TransactionType = StockTransactionType.Adjustment,
            Quantity = difference,
            BalanceQuantity = dto.NewQuantity,
            ReferenceNumber = $"ADJ-{Guid.NewGuid():N}",
            TransactionDate = adjustment.AdjustmentDate
        };

        _context.StockTransactions.Add(transaction);

        // 7. Save stock + adjustment + transaction together
        await _context.SaveChangesAsync();

        // 8. Create Audit Log
        await _auditLogService.LogAsync(
            action: "Adjustment",
            entityName: "StockAdjustment",
            entityId: adjustment.Id,
            description:
                $"Stock adjusted for Medicine ID {dto.MedicineId}, " +
                $"Batch ID {dto.BatchId} at '{dto.Location}'. " +
                $"Previous quantity: {previousQuantity}, " +
                $"New quantity: {dto.NewQuantity}, " +
                $"Difference: {difference}. " +
                $"Reason: {dto.Reason ?? "Not specified"}."
        );

        // 9. Return DTO
        return new StockAdjustmentDto
        {
            Id = adjustment.Id,
            MedicineId = adjustment.MedicineId,
            BatchId = adjustment.BatchId,
            Location = adjustment.Location,
            PreviousQuantity = adjustment.PreviousQuantity,
            NewQuantity = adjustment.NewQuantity,
            Difference = adjustment.Difference,
            Reason = adjustment.Reason,
            AdjustmentDate = adjustment.AdjustmentDate,
            CreatedAt = adjustment.CreatedAt
        };
    }
}



//using BKMC.Pharmacy.Application.DTOs.StockAdjustments;
//using BKMC.Pharmacy.Application.Interfaces;
//using BKMC.Pharmacy.Domain.Entities;
//using BKMC.Pharmacy.Domain.Enums;
//using BKMC.Pharmacy.Infrastructure.Data;
//using Microsoft.EntityFrameworkCore;

//namespace BKMC.Pharmacy.Infrastructure.Services;

//public class StockAdjustmentService : IStockAdjustmentService
//{
//    private readonly ApplicationDbContext _context;

//    public StockAdjustmentService(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<IEnumerable<StockAdjustmentDto>> GetAllAsync()
//    {
//        return await _context.StockAdjustments
//            .AsNoTracking()
//            .Select(x => new StockAdjustmentDto
//            {
//                Id = x.Id,
//                MedicineId = x.MedicineId,
//                BatchId = x.BatchId,
//                Location = x.Location,
//                PreviousQuantity = x.PreviousQuantity,
//                NewQuantity = x.NewQuantity,
//                Difference = x.Difference,
//                Reason = x.Reason,
//                AdjustmentDate = x.AdjustmentDate,
//                CreatedAt = x.CreatedAt
//            })
//            .ToListAsync();
//    }

//    public async Task<StockAdjustmentDto?> GetByIdAsync(int id)
//    {
//        return await _context.StockAdjustments
//            .AsNoTracking()
//            .Where(x => x.Id == id)
//            .Select(x => new StockAdjustmentDto
//            {
//                Id = x.Id,
//                MedicineId = x.MedicineId,
//                BatchId = x.BatchId,
//                Location = x.Location,
//                PreviousQuantity = x.PreviousQuantity,
//                NewQuantity = x.NewQuantity,
//                Difference = x.Difference,
//                Reason = x.Reason,
//                AdjustmentDate = x.AdjustmentDate,
//                CreatedAt = x.CreatedAt
//            })
//            .FirstOrDefaultAsync();
//    }

//    public async Task<StockAdjustmentDto> CreateAsync(
//        CreateStockAdjustmentDto dto)
//    {
//        // 1. Find existing stock
//        var stock = await _context.Stocks
//            .FirstOrDefaultAsync(x =>
//                x.MedicineId == dto.MedicineId &&
//                x.BatchId == dto.BatchId &&
//                x.Location == dto.Location);

//        if (stock == null)
//        {
//            throw new InvalidOperationException(
//                "Stock record was not found for the specified medicine, batch and location.");
//        }

//        // 2. Get previous quantity
//        var previousQuantity = stock.Quantity;

//        // 3. Calculate difference
//        var difference = dto.NewQuantity - previousQuantity;

//        // 4. Update stock
//        stock.Quantity = dto.NewQuantity;
//        stock.UpdatedAt = DateTime.UtcNow;

//        // 5. Create adjustment record
//        var adjustment = new StockAdjustment
//        {
//            MedicineId = dto.MedicineId,
//            BatchId = dto.BatchId,
//            Location = dto.Location,
//            PreviousQuantity = previousQuantity,
//            NewQuantity = dto.NewQuantity,
//            Difference = difference,
//            Reason = dto.Reason,
//            AdjustmentDate = dto.AdjustmentDate == default
//                ? DateTime.UtcNow
//                : dto.AdjustmentDate,
//            CreatedAt = DateTime.UtcNow
//        };

//        _context.StockAdjustments.Add(adjustment);

//        // 6. Create stock transaction
//        var transaction = new StockTransaction
//        {
//            MedicineId = dto.MedicineId,
//            BatchId = dto.BatchId,
//            Location = dto.Location,
//            TransactionType = StockTransactionType.Adjustment,
//            Quantity = difference,
//            BalanceQuantity = dto.NewQuantity,
//            ReferenceNumber = $"ADJ-{Guid.NewGuid():N}",
//            TransactionDate = adjustment.AdjustmentDate
//        };

//        _context.StockTransactions.Add(transaction);

//        // 7. Save all changes together
//        await _context.SaveChangesAsync();

//        // 8. Return DTO
//        return new StockAdjustmentDto
//        {
//            Id = adjustment.Id,
//            MedicineId = adjustment.MedicineId,
//            BatchId = adjustment.BatchId,
//            Location = adjustment.Location,
//            PreviousQuantity = adjustment.PreviousQuantity,
//            NewQuantity = adjustment.NewQuantity,
//            Difference = adjustment.Difference,
//            Reason = adjustment.Reason,
//            AdjustmentDate = adjustment.AdjustmentDate,
//            CreatedAt = adjustment.CreatedAt
//        };
//    }
//}