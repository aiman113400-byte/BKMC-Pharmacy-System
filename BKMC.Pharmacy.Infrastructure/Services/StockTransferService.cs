using BKMC.Pharmacy.Application.DTOs.StockTransfers;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Domain.Enums;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Application.Services;

public class StockTransferService : IStockTransferService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public StockTransferService(
        ApplicationDbContext context,
        IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<IEnumerable<StockTransferDto>> GetAllAsync()
    {
        return await _context.StockTransfers
            .AsNoTracking()
            .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
            .Include(x => x.Items)
                .ThenInclude(x => x.Batch)
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<StockTransferDto?> GetByIdAsync(int id)
    {
        var transfer = await _context.StockTransfers
            .AsNoTracking()
            .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
            .Include(x => x.Items)
                .ThenInclude(x => x.Batch)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (transfer == null)
            return null;

        return MapToDto(transfer);
    }

    public async Task<StockTransferDto> CreateAsync(
        CreateStockTransferDto dto)
    {
        // 1. Validate locations
        if (dto.FromLocation == dto.ToLocation)
        {
            throw new InvalidOperationException(
                "Source and destination locations cannot be the same.");
        }

        // 2. Validate items
        if (dto.Items == null || dto.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one transfer item is required.");
        }

        // 3. Validate reference number
        if (string.IsNullOrWhiteSpace(dto.ReferenceNumber))
        {
            throw new InvalidOperationException(
                "Reference number is required.");
        }

        // 4. Check duplicate reference number
        var referenceExists = await _context.StockTransfers
            .AnyAsync(x =>
                x.ReferenceNumber == dto.ReferenceNumber);

        if (referenceExists)
        {
            throw new InvalidOperationException(
                "Transfer reference number already exists.");
        }

        // 5. Create transfer header
        var transfer = new StockTransfer
        {
            FromLocation = dto.FromLocation,
            ToLocation = dto.ToLocation,
            TransferDate = dto.TransferDate,
            ReferenceNumber = dto.ReferenceNumber,
            Remarks = dto.Remarks,
            CreatedAt = DateTime.UtcNow
        };

        // 6. Validate and process every item
        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException(
                    "Transfer quantity must be greater than zero.");
            }

            // Medicine must exist and be active
            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(x =>
                    x.Id == item.MedicineId &&
                    x.IsActive);

            if (medicine == null)
            {
                throw new InvalidOperationException(
                    $"Medicine with ID {item.MedicineId} not found or inactive.");
            }

            // Batch must belong to medicine
            var batch = await _context.Batches
                .FirstOrDefaultAsync(x =>
                    x.Id == item.BatchId &&
                    x.MedicineId == item.MedicineId);

            if (batch == null)
            {
                throw new InvalidOperationException(
                    $"Batch with ID {item.BatchId} does not belong to the selected medicine.");
            }

            // Do not transfer expired batch
            if (batch.ExpiryDate.Date <= DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException(
                    $"Batch {batch.BatchNumber} is expired.");
            }

            // Find source stock
            var sourceStock = await _context.Stocks
                .FirstOrDefaultAsync(x =>
                    x.MedicineId == item.MedicineId &&
                    x.BatchId == item.BatchId &&
                    x.Location == dto.FromLocation);

            if (sourceStock == null)
            {
                throw new InvalidOperationException(
                    $"No stock available at {dto.FromLocation} for batch {batch.BatchNumber}.");
            }

            // Insufficient stock
            if (sourceStock.Quantity < item.Quantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for batch {batch.BatchNumber}. " +
                    $"Available: {sourceStock.Quantity}, Requested: {item.Quantity}.");
            }

            // -----------------------------------------
            // SOURCE STOCK
            // -----------------------------------------

            sourceStock.Quantity -= item.Quantity;
            sourceStock.UpdatedAt = DateTime.UtcNow;

            // Create TransferOut transaction
            var transferOutTransaction = new StockTransaction
            {
                MedicineId = item.MedicineId,
                BatchId = item.BatchId,
                Location = dto.FromLocation,
                TransactionType = StockTransactionType.TransferOut,
                Quantity = item.Quantity,
                BalanceQuantity = sourceStock.Quantity,
                ReferenceNumber = dto.ReferenceNumber,
                TransactionDate = dto.TransferDate
            };

            _context.StockTransactions.Add(transferOutTransaction);

            // -----------------------------------------
            // DESTINATION STOCK
            // -----------------------------------------

            var destinationStock = await _context.Stocks
                .FirstOrDefaultAsync(x =>
                    x.MedicineId == item.MedicineId &&
                    x.BatchId == item.BatchId &&
                    x.Location == dto.ToLocation);

            if (destinationStock == null)
            {
                destinationStock = new Stock
                {
                    MedicineId = item.MedicineId,
                    BatchId = item.BatchId,
                    Location = dto.ToLocation,
                    Quantity = item.Quantity,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Stocks.Add(destinationStock);
            }
            else
            {
                destinationStock.Quantity += item.Quantity;
                destinationStock.UpdatedAt = DateTime.UtcNow;
            }

            // Create TransferIn transaction
            var transferInTransaction = new StockTransaction
            {
                MedicineId = item.MedicineId,
                BatchId = item.BatchId,
                Location = dto.ToLocation,
                TransactionType = StockTransactionType.TransferIn,
                Quantity = item.Quantity,
                BalanceQuantity = destinationStock.Quantity,
                ReferenceNumber = dto.ReferenceNumber,
                TransactionDate = dto.TransferDate
            };

            _context.StockTransactions.Add(transferInTransaction);

            // -----------------------------------------
            // TRANSFER ITEM
            // -----------------------------------------

            var transferItem = new StockTransferItem
            {
                StockTransfer = transfer,
                MedicineId = item.MedicineId,
                BatchId = item.BatchId,
                Quantity = item.Quantity
            };

            transfer.Items.Add(transferItem);
        }

        _context.StockTransfers.Add(transfer);

        // Save stock + transactions + transfer together
        await _context.SaveChangesAsync();

        // -----------------------------------------
        // AUDIT LOG
        // -----------------------------------------

        await _auditLogService.LogAsync(
            action: "Transfer",
            entityName: "StockTransfer",
            entityId: transfer.Id,
            description:
                $"Stock transferred from '{dto.FromLocation}' to '{dto.ToLocation}' " +
                $"with reference '{dto.ReferenceNumber}' containing {dto.Items.Count} item(s)."
        );

        var createdTransfer = await _context.StockTransfers
            .AsNoTracking()
            .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
            .Include(x => x.Items)
                .ThenInclude(x => x.Batch)
            .FirstAsync(x => x.Id == transfer.Id);

        return MapToDto(createdTransfer);
    }

    private static StockTransferDto MapToDto(
        StockTransfer transfer)
    {
        return new StockTransferDto
        {
            Id = transfer.Id,
            FromLocation = transfer.FromLocation,
            ToLocation = transfer.ToLocation,
            TransferDate = transfer.TransferDate,
            ReferenceNumber = transfer.ReferenceNumber,
            Remarks = transfer.Remarks,
            CreatedAt = transfer.CreatedAt,

            Items = transfer.Items
                .Select(item => new StockTransferItemDto
                {
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
//using BKMC.Pharmacy.Application.DTOs.StockTransfers;
//using BKMC.Pharmacy.Application.Interfaces;
//using BKMC.Pharmacy.Domain.Entities;
//using BKMC.Pharmacy.Domain.Enums;
//using BKMC.Pharmacy.Infrastructure.Data;
//using Microsoft.EntityFrameworkCore;

//namespace BKMC.Pharmacy.Application.Services;

//public class StockTransferService : IStockTransferService
//{
//    private readonly ApplicationDbContext _context;

//    public StockTransferService(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<IEnumerable<StockTransferDto>> GetAllAsync()
//    {
//        return await _context.StockTransfers
//            .AsNoTracking()
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Medicine)
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Batch)
//            .Select(x => MapToDto(x))
//            .ToListAsync();
//    }

//    public async Task<StockTransferDto?> GetByIdAsync(int id)
//    {
//        var transfer = await _context.StockTransfers
//            .AsNoTracking()
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Medicine)
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Batch)
//            .FirstOrDefaultAsync(x => x.Id == id);

//        if (transfer == null)
//            return null;

//        return MapToDto(transfer);
//    }

//    public async Task<StockTransferDto> CreateAsync(
//        CreateStockTransferDto dto)
//    {
//        // 1. Validate locations
//        if (dto.FromLocation == dto.ToLocation)
//        {
//            throw new InvalidOperationException(
//                "Source and destination locations cannot be the same.");
//        }

//        // 2. Validate items
//        if (dto.Items == null || dto.Items.Count == 0)
//        {
//            throw new InvalidOperationException(
//                "At least one transfer item is required.");
//        }

//        // 3. Validate reference number
//        if (string.IsNullOrWhiteSpace(dto.ReferenceNumber))
//        {
//            throw new InvalidOperationException(
//                "Reference number is required.");
//        }

//        // 4. Check duplicate reference number
//        var referenceExists = await _context.StockTransfers
//            .AnyAsync(x =>
//                x.ReferenceNumber == dto.ReferenceNumber);

//        if (referenceExists)
//        {
//            throw new InvalidOperationException(
//                "Transfer reference number already exists.");
//        }

//        // 5. Create transfer header
//        var transfer = new StockTransfer
//        {
//            FromLocation = dto.FromLocation,
//            ToLocation = dto.ToLocation,
//            TransferDate = dto.TransferDate,
//            ReferenceNumber = dto.ReferenceNumber,
//            Remarks = dto.Remarks,
//            CreatedAt = DateTime.UtcNow
//        };

//        // 6. Validate and process every item
//        foreach (var item in dto.Items)
//        {
//            if (item.Quantity <= 0)
//            {
//                throw new InvalidOperationException(
//                    "Transfer quantity must be greater than zero.");
//            }

//            // Medicine must exist and be active
//            var medicine = await _context.Medicines
//                .FirstOrDefaultAsync(x =>
//                    x.Id == item.MedicineId &&
//                    x.IsActive);

//            if (medicine == null)
//            {
//                throw new InvalidOperationException(
//                    $"Medicine with ID {item.MedicineId} not found or inactive.");
//            }

//            // Batch must belong to medicine
//            var batch = await _context.Batches
//                .FirstOrDefaultAsync(x =>
//                    x.Id == item.BatchId &&
//                    x.MedicineId == item.MedicineId);

//            if (batch == null)
//            {
//                throw new InvalidOperationException(
//                    $"Batch with ID {item.BatchId} does not belong to the selected medicine.");
//            }

//            // Do not transfer expired batch
//            if (batch.ExpiryDate.Date <= DateTime.UtcNow.Date)
//            {
//                throw new InvalidOperationException(
//                    $"Batch {batch.BatchNumber} is expired.");
//            }

//            // Find source stock
//            var sourceStock = await _context.Stocks
//                .FirstOrDefaultAsync(x =>
//                    x.MedicineId == item.MedicineId &&
//                    x.BatchId == item.BatchId &&
//                    x.Location == dto.FromLocation);

//            if (sourceStock == null)
//            {
//                throw new InvalidOperationException(
//                    $"No stock available at {dto.FromLocation} for batch {batch.BatchNumber}.");
//            }

//            // Insufficient stock
//            if (sourceStock.Quantity < item.Quantity)
//            {
//                throw new InvalidOperationException(
//                    $"Insufficient stock for batch {batch.BatchNumber}. " +
//                    $"Available: {sourceStock.Quantity}, Requested: {item.Quantity}.");
//            }

//            // -----------------------------------------
//            // SOURCE STOCK
//            // -----------------------------------------

//            sourceStock.Quantity -= item.Quantity;
//            sourceStock.UpdatedAt = DateTime.UtcNow;

//            // Create TransferOut transaction
//            var transferOutTransaction = new StockTransaction
//            {
//                MedicineId = item.MedicineId,
//                BatchId = item.BatchId,
//                Location = dto.FromLocation,
//                TransactionType = StockTransactionType.TransferOut,
//                Quantity = item.Quantity,
//                BalanceQuantity = sourceStock.Quantity,
//                ReferenceNumber = dto.ReferenceNumber,
//                TransactionDate = dto.TransferDate
//            };

//            _context.StockTransactions.Add(transferOutTransaction);

//            // -----------------------------------------
//            // DESTINATION STOCK
//            // -----------------------------------------

//            var destinationStock = await _context.Stocks
//                .FirstOrDefaultAsync(x =>
//                    x.MedicineId == item.MedicineId &&
//                    x.BatchId == item.BatchId &&
//                    x.Location == dto.ToLocation);

//            if (destinationStock == null)
//            {
//                destinationStock = new Stock
//                {
//                    MedicineId = item.MedicineId,
//                    BatchId = item.BatchId,
//                    Location = dto.ToLocation,
//                    Quantity = item.Quantity,
//                    UpdatedAt = DateTime.UtcNow
//                };

//                _context.Stocks.Add(destinationStock);
//            }
//            else
//            {
//                destinationStock.Quantity += item.Quantity;
//                destinationStock.UpdatedAt = DateTime.UtcNow;
//            }

//            // Create TransferIn transaction
//            var transferInTransaction = new StockTransaction
//            {
//                MedicineId = item.MedicineId,
//                BatchId = item.BatchId,
//                Location = dto.ToLocation,
//                TransactionType = StockTransactionType.TransferIn,
//                Quantity = item.Quantity,
//                BalanceQuantity = destinationStock.Quantity,
//                ReferenceNumber = dto.ReferenceNumber,
//                TransactionDate = dto.TransferDate
//            };

//            _context.StockTransactions.Add(transferInTransaction);

//            // -----------------------------------------
//            // TRANSFER ITEM
//            // -----------------------------------------

//            var transferItem = new StockTransferItem
//            {
//                StockTransfer = transfer,
//                MedicineId = item.MedicineId,
//                BatchId = item.BatchId,
//                Quantity = item.Quantity
//            };

//            transfer.Items.Add(transferItem);
//        }

//        _context.StockTransfers.Add(transfer);

//        // Save stock + transactions + transfer together
//        await _context.SaveChangesAsync();

//        var createdTransfer = await _context.StockTransfers
//            .AsNoTracking()
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Medicine)
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Batch)
//            .FirstAsync(x => x.Id == transfer.Id);

//        return MapToDto(createdTransfer);
//    }

//    private static StockTransferDto MapToDto(
//        StockTransfer transfer)
//    {
//        return new StockTransferDto
//        {
//            Id = transfer.Id,
//            FromLocation = transfer.FromLocation,
//            ToLocation = transfer.ToLocation,
//            TransferDate = transfer.TransferDate,
//            ReferenceNumber = transfer.ReferenceNumber,
//            Remarks = transfer.Remarks,
//            CreatedAt = transfer.CreatedAt,

//            Items = transfer.Items
//                .Select(item => new StockTransferItemDto
//                {
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

