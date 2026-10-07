using BKMC.Pharmacy.Application.DTOs.Return;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Domain.Enums;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Application.Services;

public class ReturnService : IReturnService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public ReturnService(
        ApplicationDbContext context,
        IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<IEnumerable<ReturnDto>> GetAllAsync()
    {
        var returns = await _context.Returns
            .AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.Dispensing)
            .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
            .Include(x => x.Items)
                .ThenInclude(x => x.Batch)
            .OrderByDescending(x => x.ReturnDate)
            .ToListAsync();

        return returns.Select(MapToDto);
    }

    public async Task<ReturnDto?> GetByIdAsync(int id)
    {
        var returnRecord = await _context.Returns
            .AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.Dispensing)
            .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
            .Include(x => x.Items)
                .ThenInclude(x => x.Batch)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (returnRecord == null)
        {
            return null;
        }

        return MapToDto(returnRecord);
    }

    public async Task<ReturnDto> CreateAsync(CreateReturnDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one return item is required.");
        }

        if (dto.ReturnDate == default)
        {
            throw new InvalidOperationException(
                "Return date is required.");
        }

        var patient = await _context.Patients
            .FirstOrDefaultAsync(x => x.Id == dto.PatientId);

        if (patient == null)
        {
            throw new InvalidOperationException(
                "Patient not found.");
        }

        // Prevent the same DispensingItem from being returned
        // more than once inside the same request.
        var duplicateItem = dto.Items
            .GroupBy(x => x.DispensingItemId)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateItem != null)
        {
            throw new InvalidOperationException(
                $"DispensingItemId {duplicateItem.Key} " +
                "is duplicated in the same return request.");
        }

        var dispensingItemIds = dto.Items
            .Select(x => x.DispensingItemId)
            .ToList();

        var dispensingItems = await _context.DispensingItems
            .Include(x => x.Dispensing)
            .Include(x => x.Medicine)
            .Include(x => x.Batch)
            .Where(x => dispensingItemIds.Contains(x.Id))
            .ToListAsync();

        if (dispensingItems.Count != dispensingItemIds.Count)
        {
            throw new InvalidOperationException(
                "One or more dispensing items were not found.");
        }

        // All returned items must belong to the same dispensing
        // when a DispensingId is supplied.
        var dispensingIds = dispensingItems
            .Select(x => x.DispensingId)
            .Distinct()
            .ToList();

        if (dispensingIds.Count != 1)
        {
            throw new InvalidOperationException(
                "All return items must belong to the same dispensing record.");
        }

        var originalDispensingId = dispensingIds[0];

        if (dto.DispensingId.HasValue &&
            dto.DispensingId.Value != originalDispensingId)
        {
            throw new InvalidOperationException(
                "The selected dispensing does not match the return items.");
        }

        var dispensing = await _context.Dispensings
            .Include(x => x.Patient)
            .FirstOrDefaultAsync(x => x.Id == originalDispensingId);

        if (dispensing == null)
        {
            throw new InvalidOperationException(
                "Dispensing record not found.");
        }

        if (dispensing.PatientId != dto.PatientId)
        {
            throw new InvalidOperationException(
                "The dispensing record does not belong to the selected patient.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var returnEntity = new Return
            {
                PatientId = dto.PatientId,
                DispensingId = originalDispensingId,
                ReturnDate = dto.ReturnDate,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var requestItem in dto.Items)
            {
                if (requestItem.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        $"Return quantity for DispensingItemId " +
                        $"{requestItem.DispensingItemId} must be greater than zero.");
                }

                var dispensingItem = dispensingItems
                    .First(x => x.Id == requestItem.DispensingItemId);

                var alreadyReturnedQuantity = await _context.ReturnItems
                    .Where(x =>
                        x.DispensingItemId ==
                        requestItem.DispensingItemId)
                    .SumAsync(x => (decimal?)x.Quantity) ?? 0m;

                var remainingReturnableQuantity =
                    dispensingItem.Quantity -
                    alreadyReturnedQuantity;

                if (requestItem.Quantity >
                    remainingReturnableQuantity)
                {
                    throw new InvalidOperationException(
                        $"Return quantity for DispensingItemId " +
                        $"{requestItem.DispensingItemId} exceeds the " +
                        $"remaining returnable quantity. " +
                        $"Remaining: {remainingReturnableQuantity}.");
                }

                var stock = await _context.Stocks
                    .FirstOrDefaultAsync(x =>
                        x.MedicineId == dispensingItem.MedicineId &&
                        x.BatchId == dispensingItem.BatchId &&
                        x.Location == StockLocation.Pharmacy);

                if (stock == null)
                {
                    throw new InvalidOperationException(
                        $"Pharmacy stock was not found for " +
                        $"MedicineId {dispensingItem.MedicineId}, " +
                        $"BatchId {dispensingItem.BatchId}.");
                }

                stock.Quantity += requestItem.Quantity;
                stock.UpdatedAt = DateTime.UtcNow;

                var stockTransaction = new StockTransaction
                {
                    MedicineId = dispensingItem.MedicineId,
                    BatchId = dispensingItem.BatchId,
                    Location = StockLocation.Pharmacy,
                    TransactionType = StockTransactionType.Return,
                    Quantity = requestItem.Quantity,
                    BalanceQuantity = stock.Quantity,
                    ReferenceNumber = $"RET-{Guid.NewGuid():N}",
                    TransactionDate = dto.ReturnDate
                };

                _context.StockTransactions.Add(stockTransaction);

                returnEntity.Items.Add(new ReturnItem
                {
                    DispensingItemId = dispensingItem.Id,
                    MedicineId = dispensingItem.MedicineId,
                    BatchId = dispensingItem.BatchId,
                    Quantity = requestItem.Quantity
                });
            }

            _context.Returns.Add(returnEntity);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // -----------------------------------------------------
            // Audit Log
            // -----------------------------------------------------
            await _auditLogService.LogAsync(
                action: "Return",
                entityName: "Return",
                entityId: returnEntity.Id,
                description:
                    $"Return created for patient " +
                    $"'{patient.Name}' (MR: {patient.MRNumber}) " +
                    $"with {returnEntity.Items.Count} item(s)." +
                    (!string.IsNullOrWhiteSpace(returnEntity.Reason)
                        ? $" Reason: {returnEntity.Reason}."
                        : "")
            );

            var createdReturn = await _context.Returns
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.Dispensing)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Medicine)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Batch)
                .FirstAsync(x => x.Id == returnEntity.Id);

            return MapToDto(createdReturn);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static ReturnDto MapToDto(Return returnEntity)
    {
        return new ReturnDto
        {
            Id = returnEntity.Id,
            PatientId = returnEntity.PatientId,
            MRNumber = returnEntity.Patient.MRNumber,
            PatientName = returnEntity.Patient.Name,
            DispensingId = returnEntity.DispensingId,
            ReturnDate = returnEntity.ReturnDate,
            Reason = returnEntity.Reason,
            CreatedAt = returnEntity.CreatedAt,

            Items = returnEntity.Items
                .Select(item => new ReturnItemDto
                {
                    Id = item.Id,
                    DispensingItemId = item.DispensingItemId,
                    MedicineId = item.MedicineId,
                    MedicineName = item.Medicine.Name,
                    BatchId = item.BatchId,
                    BatchNumber = item.Batch.BatchNumber,
                    Quantity = item.Quantity
                })
                .ToList()
        };
    }
}
//using BKMC.Pharmacy.Application.DTOs.Return;
//using BKMC.Pharmacy.Application.Interfaces;
//using BKMC.Pharmacy.Domain.Entities;
//using BKMC.Pharmacy.Domain.Enums;
//using BKMC.Pharmacy.Infrastructure.Data;
//using Microsoft.EntityFrameworkCore;

//namespace BKMC.Pharmacy.Application.Services;

//public class ReturnService : IReturnService
//{
//    private readonly ApplicationDbContext _context;

//    public ReturnService(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<IEnumerable<ReturnDto>> GetAllAsync()
//    {
//        var returns = await _context.Returns
//            .AsNoTracking()
//            .Include(x => x.Patient)
//            .Include(x => x.Dispensing)
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Medicine)
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Batch)
//            .OrderByDescending(x => x.ReturnDate)
//            .ToListAsync();

//        return returns.Select(MapToDto);
//    }

//    public async Task<ReturnDto?> GetByIdAsync(int id)
//    {
//        var returnRecord = await _context.Returns
//            .AsNoTracking()
//            .Include(x => x.Patient)
//            .Include(x => x.Dispensing)
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Medicine)
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Batch)
//            .FirstOrDefaultAsync(x => x.Id == id);

//        if (returnRecord == null)
//        {
//            return null;
//        }

//        return MapToDto(returnRecord);
//    }

//    public async Task<ReturnDto> CreateAsync(CreateReturnDto dto)
//    {
//        if (dto.Items == null || dto.Items.Count == 0)
//        {
//            throw new InvalidOperationException(
//                "At least one return item is required.");
//        }

//        if (dto.ReturnDate == default)
//        {
//            throw new InvalidOperationException(
//                "Return date is required.");
//        }

//        var patient = await _context.Patients
//            .FirstOrDefaultAsync(x => x.Id == dto.PatientId);

//        if (patient == null)
//        {
//            throw new InvalidOperationException(
//                "Patient not found.");
//        }

//        // Prevent the same DispensingItem from being returned
//        // more than once inside the same request.
//        var duplicateItem = dto.Items
//            .GroupBy(x => x.DispensingItemId)
//            .FirstOrDefault(g => g.Count() > 1);

//        if (duplicateItem != null)
//        {
//            throw new InvalidOperationException(
//                $"DispensingItemId {duplicateItem.Key} " +
//                "is duplicated in the same return request.");
//        }

//        var dispensingItemIds = dto.Items
//            .Select(x => x.DispensingItemId)
//            .ToList();

//        var dispensingItems = await _context.DispensingItems
//            .Include(x => x.Dispensing)
//            .Include(x => x.Medicine)
//            .Include(x => x.Batch)
//            .Where(x => dispensingItemIds.Contains(x.Id))
//            .ToListAsync();

//        if (dispensingItems.Count != dispensingItemIds.Count)
//        {
//            throw new InvalidOperationException(
//                "One or more dispensing items were not found.");
//        }

//        // All returned items must belong to the same dispensing
//        // when a DispensingId is supplied.
//        var dispensingIds = dispensingItems
//            .Select(x => x.DispensingId)
//            .Distinct()
//            .ToList();

//        if (dispensingIds.Count != 1)
//        {
//            throw new InvalidOperationException(
//                "All return items must belong to the same dispensing record.");
//        }

//        var originalDispensingId = dispensingIds[0];

//        if (dto.DispensingId.HasValue &&
//            dto.DispensingId.Value != originalDispensingId)
//        {
//            throw new InvalidOperationException(
//                "The selected dispensing does not match the return items.");
//        }

//        var dispensing = await _context.Dispensings
//            .Include(x => x.Patient)
//            .FirstOrDefaultAsync(x => x.Id == originalDispensingId);

//        if (dispensing == null)
//        {
//            throw new InvalidOperationException(
//                "Dispensing record not found.");
//        }

//        if (dispensing.PatientId != dto.PatientId)
//        {
//            throw new InvalidOperationException(
//                "The dispensing record does not belong to the selected patient.");
//        }

//        await using var transaction =
//            await _context.Database.BeginTransactionAsync();

//        try
//        {
//            var returnEntity = new Return
//            {
//                PatientId = dto.PatientId,
//                DispensingId = originalDispensingId,
//                ReturnDate = dto.ReturnDate,
//                Reason = dto.Reason,
//                CreatedAt = DateTime.UtcNow
//            };

//            foreach (var requestItem in dto.Items)
//            {
//                if (requestItem.Quantity <= 0)
//                {
//                    throw new InvalidOperationException(
//                        $"Return quantity for DispensingItemId " +
//                        $"{requestItem.DispensingItemId} must be greater than zero.");
//                }

//                var dispensingItem = dispensingItems
//                    .First(x => x.Id == requestItem.DispensingItemId);

//                var alreadyReturnedQuantity = await _context.ReturnItems
//                    .Where(x =>
//                        x.DispensingItemId ==
//                        requestItem.DispensingItemId)
//                    .SumAsync(x => (decimal?)x.Quantity) ?? 0m;

//                var remainingReturnableQuantity =
//                    dispensingItem.Quantity -
//                    alreadyReturnedQuantity;

//                if (requestItem.Quantity >
//                    remainingReturnableQuantity)
//                {
//                    throw new InvalidOperationException(
//                        $"Return quantity for DispensingItemId " +
//                        $"{requestItem.DispensingItemId} exceeds the " +
//                        $"remaining returnable quantity. " +
//                        $"Remaining: {remainingReturnableQuantity}.");
//                }

//                var stock = await _context.Stocks
//                    .FirstOrDefaultAsync(x =>
//                        x.MedicineId == dispensingItem.MedicineId &&
//                        x.BatchId == dispensingItem.BatchId &&
//                        x.Location == StockLocation.Pharmacy);

//                if (stock == null)
//                {
//                    throw new InvalidOperationException(
//                        $"Pharmacy stock was not found for " +
//                        $"MedicineId {dispensingItem.MedicineId}, " +
//                        $"BatchId {dispensingItem.BatchId}.");
//                }

//                stock.Quantity += requestItem.Quantity;
//                stock.UpdatedAt = DateTime.UtcNow;

//                var stockTransaction = new StockTransaction
//                {
//                    MedicineId = dispensingItem.MedicineId,
//                    BatchId = dispensingItem.BatchId,
//                    Location = StockLocation.Pharmacy,
//                    TransactionType = StockTransactionType.Return,
//                    Quantity = requestItem.Quantity,
//                    BalanceQuantity = stock.Quantity,
//                    ReferenceNumber = $"RET-{Guid.NewGuid():N}",
//                    TransactionDate = dto.ReturnDate
//                };

//                _context.StockTransactions.Add(stockTransaction);

//                returnEntity.Items.Add(new ReturnItem
//                {
//                    DispensingItemId = dispensingItem.Id,
//                    MedicineId = dispensingItem.MedicineId,
//                    BatchId = dispensingItem.BatchId,
//                    Quantity = requestItem.Quantity
//                });
//            }

//            _context.Returns.Add(returnEntity);

//            await _context.SaveChangesAsync();

//            await transaction.CommitAsync();

//            var createdReturn = await _context.Returns
//                .AsNoTracking()
//                .Include(x => x.Patient)
//                .Include(x => x.Dispensing)
//                .Include(x => x.Items)
//                    .ThenInclude(x => x.Medicine)
//                .Include(x => x.Items)
//                    .ThenInclude(x => x.Batch)
//                .FirstAsync(x => x.Id == returnEntity.Id);

//            return MapToDto(createdReturn);
//        }
//        catch
//        {
//            await transaction.RollbackAsync();
//            throw;
//        }
//    }

//    private static ReturnDto MapToDto(Return returnEntity)
//    {
//        return new ReturnDto
//        {
//            Id = returnEntity.Id,
//            PatientId = returnEntity.PatientId,
//            MRNumber = returnEntity.Patient.MRNumber,
//            PatientName = returnEntity.Patient.Name,
//            DispensingId = returnEntity.DispensingId,
//            ReturnDate = returnEntity.ReturnDate,
//            Reason = returnEntity.Reason,
//            CreatedAt = returnEntity.CreatedAt,

//            Items = returnEntity.Items
//                .Select(item => new ReturnItemDto
//                {
//                    Id = item.Id,
//                    DispensingItemId = item.DispensingItemId,
//                    MedicineId = item.MedicineId,
//                    MedicineName = item.Medicine.Name,
//                    BatchId = item.BatchId,
//                    BatchNumber = item.Batch.BatchNumber,
//                    Quantity = item.Quantity
//                })
//                .ToList()
//        };
//    }
//}