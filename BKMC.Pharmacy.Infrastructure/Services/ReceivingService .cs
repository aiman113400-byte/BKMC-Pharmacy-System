using BKMC.Pharmacy.Application.DTOs.Receiving;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Domain.Enums;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Application.Services;

public class ReceivingService : IReceivingService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public ReceivingService(
        ApplicationDbContext context,
        IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<IEnumerable<ReceivingDto>> GetAllAsync()
    {
        return await _context.PurchaseInvoices
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.PurchaseItems)
            .ThenInclude(x => x.Batch)
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<ReceivingDto?> GetByIdAsync(int id)
    {
        var invoice = await _context.PurchaseInvoices
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.PurchaseItems)
            .ThenInclude(x => x.Batch)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null)
            return null;

        return MapToDto(invoice);
    }

    public async Task<ReceivingDto> CreateAsync(CreateReceivingDto dto)
    {
        // 1. Validate Supplier
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(x =>
                x.Id == dto.SupplierId &&
                x.IsActive);

        if (supplier == null)
            throw new InvalidOperationException(
                "Supplier not found or inactive.");

        // 2. Validate Invoice Number
        var invoiceExists = await _context.PurchaseInvoices
            .AnyAsync(x => x.InvoiceNumber == dto.InvoiceNumber);

        if (invoiceExists)
            throw new InvalidOperationException(
                "Invoice number already exists.");

        // 3. Validate Items
        if (dto.Items == null || dto.Items.Count == 0)
            throw new InvalidOperationException(
                "At least one receiving item is required.");

        // 4. Create Purchase Invoice
        var invoice = new PurchaseInvoice
        {
            SupplierId = dto.SupplierId,
            InvoiceNumber = dto.InvoiceNumber,
            InvoiceDate = dto.InvoiceDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.PurchaseInvoices.Add(invoice);

        // 5. Process Receiving Items
        foreach (var item in dto.Items)
        {
            // Quantity validation
            if (item.Quantity <= 0)
                throw new InvalidOperationException(
                    "Quantity must be greater than zero.");

            // Unit price validation
            if (item.UnitPrice < 0)
                throw new InvalidOperationException(
                    "Unit price cannot be negative.");

            // Batch number validation
            if (string.IsNullOrWhiteSpace(item.BatchNumber))
                throw new InvalidOperationException(
                    "Batch number is required.");

            // Manufacturing date validation
            if (item.ManufacturingDate > DateTime.UtcNow)
                throw new InvalidOperationException(
                    "Manufacturing date cannot be in the future.");

            // Expiry date validation
            if (item.ExpiryDate <= item.ManufacturingDate)
                throw new InvalidOperationException(
                    "Expiry date must be greater than manufacturing date.");

            // Expired batch validation
            if (item.ExpiryDate.Date <= DateTime.UtcNow.Date)
                throw new InvalidOperationException(
                    "Expired batch cannot be received.");

            // Medicine validation
            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(x =>
                    x.Id == item.MedicineId &&
                    x.IsActive);

            if (medicine == null)
                throw new InvalidOperationException(
                    $"Medicine with ID {item.MedicineId} not found or inactive.");

            // Duplicate batch validation
            var batchExists = await _context.Batches
                .AnyAsync(x =>
                    x.MedicineId == item.MedicineId &&
                    x.BatchNumber == item.BatchNumber);

            if (batchExists)
                throw new InvalidOperationException(
                    $"Batch number '{item.BatchNumber}' already exists for this medicine.");

            // 6. Create Batch
            var batch = new Batch
            {
                MedicineId = item.MedicineId,
                BatchNumber = item.BatchNumber,
                ManufacturingDate = item.ManufacturingDate,
                ExpiryDate = item.ExpiryDate,
                Quantity = item.Quantity,
                CreatedAt = DateTime.UtcNow
            };

            _context.Batches.Add(batch);

            // 7. Create Purchase Item
            var purchaseItem = new PurchaseItem
            {
                PurchaseInvoice = invoice,
                MedicineId = item.MedicineId,
                Batch = batch,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            };

            invoice.PurchaseItems.Add(purchaseItem);

            // 8. Create Warehouse Stock
            var stock = new Stock
            {
                MedicineId = item.MedicineId,
                Medicine = medicine,
                Batch = batch,
                Location = StockLocation.Warehouse,
                Quantity = item.Quantity,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Stocks.Add(stock);

            // 9. Create Receiving Stock Transaction
            var stockTransaction = new StockTransaction
            {
                MedicineId = item.MedicineId,
                Medicine = medicine,
                Batch = batch,
                Location = StockLocation.Warehouse,
                TransactionType = StockTransactionType.Receiving,
                Quantity = item.Quantity,
                BalanceQuantity = item.Quantity,
                ReferenceNumber = dto.InvoiceNumber,
                TransactionDate = dto.InvoiceDate
            };

            _context.StockTransactions.Add(stockTransaction);
        }

        // 10. Save everything together
        await _context.SaveChangesAsync();

        // 11. Create Audit Log
        await _auditLogService.LogAsync(
            action: "Create",
            entityName: "Receiving",
            entityId: invoice.Id,
            description:
                $"Receiving invoice '{invoice.InvoiceNumber}' created from supplier '{supplier.Name}' with {dto.Items.Count} item(s)."
        );

        // 12. Reload created invoice with related data
        var createdInvoice = await _context.PurchaseInvoices
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.PurchaseItems)
                .ThenInclude(x => x.Batch)
            .FirstAsync(x => x.Id == invoice.Id);

        // 13. Return DTO
        return MapToDto(createdInvoice);
    }

    private static ReceivingDto MapToDto(PurchaseInvoice invoice)
    {
        return new ReceivingDto
        {
            Id = invoice.Id,
            SupplierId = invoice.SupplierId,
            SupplierName = invoice.Supplier.Name,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            CreatedAt = invoice.CreatedAt,

            Items = invoice.PurchaseItems
                .Select(item => new ReceivingItemDto
                {
                    MedicineId = item.MedicineId,
                    BatchNumber = item.Batch.BatchNumber,
                    ManufacturingDate = item.Batch.ManufacturingDate,
                    ExpiryDate = item.Batch.ExpiryDate,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                })
                .ToList()
        };
    }
}




//using BKMC.Pharmacy.Application.DTOs.Receiving;
//using BKMC.Pharmacy.Application.Interfaces;
//using BKMC.Pharmacy.Domain.Entities;
//using BKMC.Pharmacy.Domain.Enums;
//using BKMC.Pharmacy.Infrastructure.Data;
//using Microsoft.EntityFrameworkCore;

//namespace BKMC.Pharmacy.Application.Services;

//public class ReceivingService : IReceivingService
//{
//    private readonly ApplicationDbContext _context;

//    public ReceivingService(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<IEnumerable<ReceivingDto>> GetAllAsync()
//    {
//        return await _context.PurchaseInvoices
//            .AsNoTracking()
//            .Include(x => x.Supplier)
//            .Include(x => x.PurchaseItems)
//            .ThenInclude(x => x.Batch)
//            .Select(x => MapToDto(x))
//            .ToListAsync();
//    }

//    public async Task<ReceivingDto?> GetByIdAsync(int id)
//    {
//        var invoice = await _context.PurchaseInvoices
//            .AsNoTracking()
//            .Include(x => x.Supplier)
//            .Include(x => x.PurchaseItems)
//            .ThenInclude(x => x.Batch)
//            .FirstOrDefaultAsync(x => x.Id == id);

//        if (invoice == null)
//            return null;

//        return MapToDto(invoice);
//    }

//    public async Task<ReceivingDto> CreateAsync(CreateReceivingDto dto)
//    {
//        // 1. Validate Supplier
//        var supplier = await _context.Suppliers
//            .FirstOrDefaultAsync(x =>
//                x.Id == dto.SupplierId &&
//                x.IsActive);

//        if (supplier == null)
//            throw new InvalidOperationException(
//                "Supplier not found or inactive.");

//        // 2. Validate Invoice Number
//        var invoiceExists = await _context.PurchaseInvoices
//            .AnyAsync(x => x.InvoiceNumber == dto.InvoiceNumber);

//        if (invoiceExists)
//            throw new InvalidOperationException(
//                "Invoice number already exists.");

//        // 3. Validate Items
//        if (dto.Items == null || dto.Items.Count == 0)
//            throw new InvalidOperationException(
//                "At least one receiving item is required.");

//        // 4. Create Purchase Invoice
//        var invoice = new PurchaseInvoice
//        {
//            SupplierId = dto.SupplierId,
//            InvoiceNumber = dto.InvoiceNumber,
//            InvoiceDate = dto.InvoiceDate,
//            CreatedAt = DateTime.UtcNow
//        };

//        _context.PurchaseInvoices.Add(invoice);

//        // 5. Process Receiving Items
//        foreach (var item in dto.Items)
//        {
//            // Quantity validation
//            if (item.Quantity <= 0)
//                throw new InvalidOperationException(
//                    "Quantity must be greater than zero.");

//            // Unit price validation
//            if (item.UnitPrice < 0)
//                throw new InvalidOperationException(
//                    "Unit price cannot be negative.");

//            // Batch number validation
//            if (string.IsNullOrWhiteSpace(item.BatchNumber))
//                throw new InvalidOperationException(
//                    "Batch number is required.");

//            // Manufacturing date validation
//            if (item.ManufacturingDate > DateTime.UtcNow)
//                throw new InvalidOperationException(
//                    "Manufacturing date cannot be in the future.");

//            // Expiry date validation
//            if (item.ExpiryDate <= item.ManufacturingDate)
//                throw new InvalidOperationException(
//                    "Expiry date must be greater than manufacturing date.");

//            // Expired batch validation
//            if (item.ExpiryDate.Date <= DateTime.UtcNow.Date)
//                throw new InvalidOperationException(
//                    "Expired batch cannot be received.");

//            // Medicine validation
//            var medicine = await _context.Medicines
//                .FirstOrDefaultAsync(x =>
//                    x.Id == item.MedicineId &&
//                    x.IsActive);

//            if (medicine == null)
//                throw new InvalidOperationException(
//                    $"Medicine with ID {item.MedicineId} not found or inactive.");

//            // Duplicate batch validation
//            var batchExists = await _context.Batches
//                .AnyAsync(x =>
//                    x.MedicineId == item.MedicineId &&
//                    x.BatchNumber == item.BatchNumber);

//            if (batchExists)
//                throw new InvalidOperationException(
//                    $"Batch number '{item.BatchNumber}' already exists for this medicine.");

//            // 6. Create Batch
//            var batch = new Batch
//            {
//                MedicineId = item.MedicineId,
//                BatchNumber = item.BatchNumber,
//                ManufacturingDate = item.ManufacturingDate,
//                ExpiryDate = item.ExpiryDate,
//                Quantity = item.Quantity,
//                CreatedAt = DateTime.UtcNow
//            };

//            _context.Batches.Add(batch);

//            // 7. Create Purchase Item
//            var purchaseItem = new PurchaseItem
//            {
//                PurchaseInvoice = invoice,
//                MedicineId = item.MedicineId,
//                Batch = batch,
//                Quantity = item.Quantity,
//                UnitPrice = item.UnitPrice
//            };

//            invoice.PurchaseItems.Add(purchaseItem);

//            // 8. Create Warehouse Stock
//            var stock = new Stock
//            {
//                MedicineId = item.MedicineId,
//                Medicine = medicine,

//                Batch = batch,

//                Location = StockLocation.Warehouse,
//                Quantity = item.Quantity,
//                UpdatedAt = DateTime.UtcNow
//            };

//            _context.Stocks.Add(stock);

//            // 9. Create Receiving Stock Transaction
//            var stockTransaction = new StockTransaction
//            {
//                MedicineId = item.MedicineId,
//                Medicine = medicine,

//                Batch = batch,

//                Location = StockLocation.Warehouse,
//                TransactionType = StockTransactionType.Receiving,

//                Quantity = item.Quantity,
//                BalanceQuantity = item.Quantity,

//                ReferenceNumber = dto.InvoiceNumber,
//                TransactionDate = dto.InvoiceDate
//            };

//            _context.StockTransactions.Add(stockTransaction);
//        }

//        // 10. Save everything together
//        await _context.SaveChangesAsync();

//        // 11. Reload created invoice with related data
//        var createdInvoice = await _context.PurchaseInvoices
//            .AsNoTracking()
//            .Include(x => x.Supplier)
//            .Include(x => x.PurchaseItems)
//                .ThenInclude(x => x.Batch)
//            .FirstAsync(x => x.Id == invoice.Id);

//        // 12. Return DTO
//        return MapToDto(createdInvoice);
//    }

//    private static ReceivingDto MapToDto(PurchaseInvoice invoice)
//    {
//        return new ReceivingDto
//        {
//            Id = invoice.Id,
//            SupplierId = invoice.SupplierId,
//            SupplierName = invoice.Supplier.Name,
//            InvoiceNumber = invoice.InvoiceNumber,
//            InvoiceDate = invoice.InvoiceDate,
//            CreatedAt = invoice.CreatedAt,

//            Items = invoice.PurchaseItems
//                .Select(item => new ReceivingItemDto
//                {
//                    MedicineId = item.MedicineId,
//                    BatchNumber = item.Batch.BatchNumber,
//                    ManufacturingDate = item.Batch.ManufacturingDate,
//                    ExpiryDate = item.Batch.ExpiryDate,
//                    Quantity = item.Quantity,
//                    UnitPrice = item.UnitPrice
//                })
//                .ToList()
//        };
//    }
//}