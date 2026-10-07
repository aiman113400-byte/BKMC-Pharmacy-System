using BKMC.Pharmacy.Application.DTOs.Dispensing;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Domain.Enums;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Application.Services;

public class DispensingService : IDispensingService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public DispensingService(
        ApplicationDbContext context,
        IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<IEnumerable<DispensingDto>> GetAllAsync()
    {
        var dispensings = await _context.Dispensings
            .AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
            .Include(x => x.Items)
                .ThenInclude(x => x.Batch)
            .ToListAsync();

        return dispensings.Select(MapToDto);
    }

    public async Task<DispensingDto?> GetByIdAsync(int id)
    {
        var dispensing = await _context.Dispensings
            .AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
            .Include(x => x.Items)
                .ThenInclude(x => x.Batch)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (dispensing == null)
            return null;

        return MapToDto(dispensing);
    }

    public async Task<DispensingDto> CreateAsync(
        CreateDispensingDto dto)
    {
        // 1. Validate items
        if (dto.Items == null || dto.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one dispensing item is required.");
        }

        // 2. Validate patient
        var patient = await _context.Patients
            .FirstOrDefaultAsync(x => x.Id == dto.PatientId);

        if (patient == null)
        {
            throw new InvalidOperationException(
                "Patient not found.");
        }

        // 3. Start database transaction
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var dispensing = new Dispensing
            {
                PatientId = dto.PatientId,
                DispensingDate = dto.DispensingDate,
                PrescriptionNumber =
                    string.IsNullOrWhiteSpace(dto.PrescriptionNumber)
                        ? null
                        : dto.PrescriptionNumber.Trim(),
                Remarks =
                    string.IsNullOrWhiteSpace(dto.Remarks)
                        ? null
                        : dto.Remarks.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            // Keep track of every actual stock deduction.
            var stockTransactions = new List<StockTransaction>();

            foreach (var item in dto.Items)
            {
                // 4. Quantity validation
                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        "Dispensing quantity must be greater than zero.");
                }

                // 5. Medicine validation
                var medicine = await _context.Medicines
                    .FirstOrDefaultAsync(x =>
                        x.Id == item.MedicineId &&
                        x.IsActive);

                if (medicine == null)
                {
                    throw new InvalidOperationException(
                        $"Medicine with ID {item.MedicineId} " +
                        "not found or inactive.");
                }

                Stock? selectedStock = null;

                // 6. Manual batch selected
                if (item.BatchId.HasValue)
                {
                    selectedStock = await _context.Stocks
                        .Include(x => x.Batch)
                        .FirstOrDefaultAsync(x =>
                            x.MedicineId == item.MedicineId &&
                            x.BatchId == item.BatchId.Value &&
                            x.Location == StockLocation.Pharmacy &&
                            x.Quantity > 0);

                    if (selectedStock == null)
                    {
                        throw new InvalidOperationException(
                            "Selected batch does not have available " +
                            "pharmacy stock.");
                    }

                    if (selectedStock.Batch.ExpiryDate.Date
                        <= DateTime.UtcNow.Date)
                    {
                        throw new InvalidOperationException(
                            $"Batch {selectedStock.Batch.BatchNumber} is expired.");
                    }

                    if (selectedStock.Quantity < item.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient stock for batch " +
                            $"{selectedStock.Batch.BatchNumber}. " +
                            $"Available: {selectedStock.Quantity}, " +
                            $"Requested: {item.Quantity}.");
                    }

                    // Deduct selected batch
                    selectedStock.Quantity -= item.Quantity;
                    selectedStock.UpdatedAt = DateTime.UtcNow;

                    dispensing.Items.Add(new DispensingItem
                    {
                        MedicineId = item.MedicineId,
                        BatchId = selectedStock.BatchId,
                        Quantity = item.Quantity,
                        Dosage = item.Dosage
                    });

                    // Create dispensing stock transaction
                    stockTransactions.Add(new StockTransaction
                    {
                        MedicineId = item.MedicineId,
                        BatchId = selectedStock.BatchId,
                        Location = StockLocation.Pharmacy,
                        TransactionType = StockTransactionType.Dispensing,
                        Quantity = -item.Quantity,
                        BalanceQuantity = selectedStock.Quantity,
                        TransactionDate = dispensing.DispensingDate
                    });
                }
                else
                {
                    // 7. FEFO
                    var stocks = await _context.Stocks
                        .Include(x => x.Batch)
                        .Where(x =>
                            x.MedicineId == item.MedicineId &&
                            x.Location == StockLocation.Pharmacy &&
                            x.Quantity > 0 &&
                            x.Batch.ExpiryDate.Date >
                                DateTime.UtcNow.Date)
                        .OrderBy(x => x.Batch.ExpiryDate)
                        .ToListAsync();

                    var availableQuantity =
                        stocks.Sum(x => x.Quantity);

                    if (availableQuantity < item.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient pharmacy stock for " +
                            $"{medicine.Name}. " +
                            $"Available: {availableQuantity}, " +
                            $"Requested: {item.Quantity}.");
                    }

                    decimal remainingQuantity = item.Quantity;

                    foreach (var stock in stocks)
                    {
                        if (remainingQuantity <= 0)
                            break;

                        var quantityToDispense =
                            Math.Min(
                                stock.Quantity,
                                remainingQuantity);

                        stock.Quantity -= quantityToDispense;
                        stock.UpdatedAt = DateTime.UtcNow;

                        dispensing.Items.Add(new DispensingItem
                        {
                            MedicineId = item.MedicineId,
                            BatchId = stock.BatchId,
                            Quantity = quantityToDispense,
                            Dosage = item.Dosage
                        });

                        // Create one transaction for each FEFO batch deduction.
                        stockTransactions.Add(new StockTransaction
                        {
                            MedicineId = item.MedicineId,
                            BatchId = stock.BatchId,
                            Location = StockLocation.Pharmacy,
                            TransactionType = StockTransactionType.Dispensing,
                            Quantity = -quantityToDispense,
                            BalanceQuantity = stock.Quantity,
                            TransactionDate = dispensing.DispensingDate
                        });

                        remainingQuantity -= quantityToDispense;
                    }
                }
            }

            // 8. Add dispensing
            _context.Dispensings.Add(dispensing);

            // 9. Save dispensing and stock changes first
            // This generates the Dispensing ID.
            await _context.SaveChangesAsync();

            // 10. Add stock transactions with dispensing reference
            foreach (var stockTransaction in stockTransactions)
            {
                stockTransaction.ReferenceNumber =
                    $"DISP-{dispensing.Id}";
            }

            _context.StockTransactions.AddRange(stockTransactions);

            // 11. Save stock transactions
            await _context.SaveChangesAsync();

            // 12. Commit complete business transaction
            await transaction.CommitAsync();

            // 13. Create Audit Log after successful dispensing
            await _auditLogService.LogAsync(
                action: "Dispense",
                entityName: "Dispensing",
                entityId: dispensing.Id,
                description:
                    $"Dispensing created for patient " +
                    $"'{patient.Name}' (MR: {patient.MRNumber}) " +
                    $"with {dispensing.Items.Count} item(s)." +
                    (!string.IsNullOrWhiteSpace(dispensing.PrescriptionNumber)
                        ? $" Prescription: {dispensing.PrescriptionNumber}."
                        : "")
            );

            // 14. Load created dispensing
            var createdDispensing =
                await _context.Dispensings
                    .AsNoTracking()
                    .Include(x => x.Patient)
                    .Include(x => x.Items)
                        .ThenInclude(x => x.Medicine)
                    .Include(x => x.Items)
                        .ThenInclude(x => x.Batch)
                    .FirstAsync(x => x.Id == dispensing.Id);

            return MapToDto(createdDispensing);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static DispensingDto MapToDto(
        Dispensing dispensing)
    {
        return new DispensingDto
        {
            Id = dispensing.Id,

            PatientId = dispensing.PatientId,

            MRNumber = dispensing.Patient.MRNumber,

            PatientName = dispensing.Patient.Name,

            DispensingDate =
                dispensing.DispensingDate,

            PrescriptionNumber =
                dispensing.PrescriptionNumber,

            Remarks =
                dispensing.Remarks,

            CreatedAt =
                dispensing.CreatedAt,

            Items = dispensing.Items
                .Select(item => new DispensingItemDto
                {
                    Id = item.Id,

                    MedicineId =
                        item.MedicineId,

                    MedicineName =
                        item.Medicine.Name,

                    BatchId =
                        item.BatchId,

                    BatchNumber =
                        item.Batch.BatchNumber,

                    Quantity =
                        item.Quantity,

                    Dosage =
                        item.Dosage
                })
                .ToList()
        };
    }
}

//using BKMC.Pharmacy.Application.DTOs.Dispensing;
//using BKMC.Pharmacy.Application.Interfaces;
//using BKMC.Pharmacy.Domain.Entities;
//using BKMC.Pharmacy.Domain.Enums;
//using BKMC.Pharmacy.Infrastructure.Data;
//using Microsoft.EntityFrameworkCore;

//namespace BKMC.Pharmacy.Application.Services;

//public class DispensingService : IDispensingService
//{
//    private readonly ApplicationDbContext _context;

//    public DispensingService(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<IEnumerable<DispensingDto>> GetAllAsync()
//    {
//        var dispensings = await _context.Dispensings
//            .AsNoTracking()
//            .Include(x => x.Patient)
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Medicine)
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Batch)
//            .ToListAsync();

//        return dispensings.Select(MapToDto);
//    }

//    public async Task<DispensingDto?> GetByIdAsync(int id)
//    {
//        var dispensing = await _context.Dispensings
//            .AsNoTracking()
//            .Include(x => x.Patient)
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Medicine)
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Batch)
//            .FirstOrDefaultAsync(x => x.Id == id);

//        if (dispensing == null)
//            return null;

//        return MapToDto(dispensing);
//    }

//    public async Task<DispensingDto> CreateAsync(
//        CreateDispensingDto dto)
//    {
//        // 1. Validate items
//        if (dto.Items == null || dto.Items.Count == 0)
//        {
//            throw new InvalidOperationException(
//                "At least one dispensing item is required.");
//        }

//        // 2. Validate patient
//        var patient = await _context.Patients
//            .FirstOrDefaultAsync(x => x.Id == dto.PatientId);

//        if (patient == null)
//        {
//            throw new InvalidOperationException(
//                "Patient not found.");
//        }

//        // 3. Start database transaction
//        await using var transaction =
//            await _context.Database.BeginTransactionAsync();

//        try
//        {
//            var dispensing = new Dispensing
//            {
//                PatientId = dto.PatientId,
//                DispensingDate = dto.DispensingDate,
//                PrescriptionNumber =
//                    string.IsNullOrWhiteSpace(dto.PrescriptionNumber)
//                        ? null
//                        : dto.PrescriptionNumber.Trim(),
//                Remarks =
//                    string.IsNullOrWhiteSpace(dto.Remarks)
//                        ? null
//                        : dto.Remarks.Trim(),
//                CreatedAt = DateTime.UtcNow
//            };

//            foreach (var item in dto.Items)
//            {
//                // 4. Quantity validation
//                if (item.Quantity <= 0)
//                {
//                    throw new InvalidOperationException(
//                        "Dispensing quantity must be greater than zero.");
//                }

//                // 5. Medicine validation
//                var medicine = await _context.Medicines
//                    .FirstOrDefaultAsync(x =>
//                        x.Id == item.MedicineId &&
//                        x.IsActive);

//                if (medicine == null)
//                {
//                    throw new InvalidOperationException(
//                        $"Medicine with ID {item.MedicineId} " +
//                        "not found or inactive.");
//                }

//                Stock? selectedStock = null;

//                // 6. Manual batch selected
//                if (item.BatchId.HasValue)
//                {
//                    selectedStock = await _context.Stocks
//                        .Include(x => x.Batch)
//                        .FirstOrDefaultAsync(x =>
//                            x.MedicineId == item.MedicineId &&
//                            x.BatchId == item.BatchId.Value &&
//                            x.Location == StockLocation.Pharmacy &&
//                            x.Quantity > 0);

//                    if (selectedStock == null)
//                    {
//                        throw new InvalidOperationException(
//                            "Selected batch does not have available " +
//                            "pharmacy stock.");
//                    }

//                    if (selectedStock.Batch.ExpiryDate.Date
//                        <= DateTime.UtcNow.Date)
//                    {
//                        throw new InvalidOperationException(
//                            $"Batch {selectedStock.Batch.BatchNumber} is expired.");
//                    }

//                    if (selectedStock.Quantity < item.Quantity)
//                    {
//                        throw new InvalidOperationException(
//                            $"Insufficient stock for batch " +
//                            $"{selectedStock.Batch.BatchNumber}. " +
//                            $"Available: {selectedStock.Quantity}, " +
//                            $"Requested: {item.Quantity}.");
//                    }

//                    // Deduct selected batch
//                    selectedStock.Quantity -= item.Quantity;
//                    selectedStock.UpdatedAt = DateTime.UtcNow;

//                    dispensing.Items.Add(new DispensingItem
//                    {
//                        MedicineId = item.MedicineId,
//                        BatchId = selectedStock.BatchId,
//                        Quantity = item.Quantity,
//                        Dosage = item.Dosage
//                    });
//                }
//                else
//                {
//                    // 7. FEFO
//                    var stocks = await _context.Stocks
//                        .Include(x => x.Batch)
//                        .Where(x =>
//                            x.MedicineId == item.MedicineId &&
//                            x.Location == StockLocation.Pharmacy &&
//                            x.Quantity > 0 &&
//                            x.Batch.ExpiryDate.Date >
//                                DateTime.UtcNow.Date)
//                        .OrderBy(x => x.Batch.ExpiryDate)
//                        .ToListAsync();

//                    var availableQuantity =
//                        stocks.Sum(x => x.Quantity);

//                    if (availableQuantity < item.Quantity)
//                    {
//                        throw new InvalidOperationException(
//                            $"Insufficient pharmacy stock for " +
//                            $"{medicine.Name}. " +
//                            $"Available: {availableQuantity}, " +
//                            $"Requested: {item.Quantity}.");
//                    }

//                    decimal remainingQuantity = item.Quantity;

//                    foreach (var stock in stocks)
//                    {
//                        if (remainingQuantity <= 0)
//                            break;

//                        var quantityToDispense =
//                            Math.Min(
//                                stock.Quantity,
//                                remainingQuantity);

//                        stock.Quantity -= quantityToDispense;
//                        stock.UpdatedAt = DateTime.UtcNow;

//                        dispensing.Items.Add(new DispensingItem
//                        {
//                            MedicineId = item.MedicineId,
//                            BatchId = stock.BatchId,
//                            Quantity = quantityToDispense,
//                            Dosage = item.Dosage
//                        });

//                        remainingQuantity -= quantityToDispense;
//                    }
//                }
//            }

//            _context.Dispensings.Add(dispensing);

//            await _context.SaveChangesAsync();

//            await transaction.CommitAsync();

//            var createdDispensing =
//                await _context.Dispensings
//                    .AsNoTracking()
//                    .Include(x => x.Patient)
//                    .Include(x => x.Items)
//                        .ThenInclude(x => x.Medicine)
//                    .Include(x => x.Items)
//                        .ThenInclude(x => x.Batch)
//                    .FirstAsync(x => x.Id == dispensing.Id);

//            return MapToDto(createdDispensing);
//        }
//        catch
//        {
//            await transaction.RollbackAsync();
//            throw;
//        }
//    }

//    private static DispensingDto MapToDto(
//        Dispensing dispensing)
//    {
//        return new DispensingDto
//        {
//            Id = dispensing.Id,

//            PatientId = dispensing.PatientId,

//            MRNumber = dispensing.Patient.MRNumber,

//            PatientName = dispensing.Patient.Name,

//            DispensingDate =
//                dispensing.DispensingDate,

//            PrescriptionNumber =
//                dispensing.PrescriptionNumber,

//            Remarks =
//                dispensing.Remarks,

//            CreatedAt =
//                dispensing.CreatedAt,

//            Items = dispensing.Items
//                .Select(item => new DispensingItemDto
//                {
//                    Id = item.Id,                          //add new code here//
//                    MedicineId =
//                        item.MedicineId,

//                    MedicineName =
//                        item.Medicine.Name,

//                    BatchId =
//                        item.BatchId,

//                    BatchNumber =
//                        item.Batch.BatchNumber,

//                    Quantity =
//                        item.Quantity,

//                    Dosage =
//                        item.Dosage
//                })
//                .ToList()
//        };
//    }
//}