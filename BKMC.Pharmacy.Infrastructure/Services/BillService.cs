using BKMC.Pharmacy.Application.DTOs.Billing;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Application.Services;

public class BillService : IBillService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public BillService(
        ApplicationDbContext context,
        IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    // =========================================================
    // GET ALL BILLS
    // =========================================================
    public async Task<IEnumerable<BillDto>> GetAllAsync()
    {
        var bills = await _context.Bills
            .AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.Dispensing)
            .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
            .OrderByDescending(x => x.BillDate)
            .ToListAsync();

        return bills.Select(MapToDto);
    }

    // =========================================================
    // GET BILL BY ID
    // =========================================================
    public async Task<BillDto?> GetByIdAsync(int id)
    {
        var bill = await _context.Bills
            .AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.Dispensing)
            .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (bill == null)
        {
            return null;
        }

        return MapToDto(bill);
    }

    // =========================================================
    // CREATE BILL
    // =========================================================
    public async Task<BillDto> CreateAsync(CreateBillDto dto)
    {
        // -----------------------------------------------------
        // 1. Validate Patient
        // -----------------------------------------------------
        var patient = await _context.Patients
            .FirstOrDefaultAsync(x => x.Id == dto.PatientId);

        if (patient == null)
        {
            throw new InvalidOperationException(
                "Patient not found.");
        }

        // -----------------------------------------------------
        // 2. Validate Bill Items
        // -----------------------------------------------------
        if (dto.Items == null || dto.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one bill item is required.");
        }

        // -----------------------------------------------------
        // 3. Validate Paid Amount
        // -----------------------------------------------------
        if (dto.PaidAmount < 0)
        {
            throw new InvalidOperationException(
                "Paid amount cannot be negative.");
        }

        // -----------------------------------------------------
        // 4. Validate Dispensing
        // -----------------------------------------------------
        if (dto.DispensingId.HasValue)
        {
            var dispensing = await _context.Dispensings
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.DispensingId.Value);

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
        }

        // -----------------------------------------------------
        // 5. Create Bill
        // -----------------------------------------------------
        var bill = new Bill
        {
            PatientId = dto.PatientId,

            DispensingId = dto.DispensingId,

            BillNumber = await GenerateBillNumberAsync(),

            BillDate = dto.BillDate,

            PaidAmount = dto.PaidAmount,

            CreatedAt = DateTime.UtcNow
        };

        decimal totalAmount = 0;

        // -----------------------------------------------------
        // 6. Create Bill Items
        // -----------------------------------------------------
        foreach (var item in dto.Items)
        {
            // Quantity validation
            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException(
                    "Bill item quantity must be greater than zero.");
            }

            // Unit price validation
            if (item.UnitPrice < 0)
            {
                throw new InvalidOperationException(
                    "Unit price cannot be negative.");
            }

            // Medicine validation
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

            // -------------------------------------------------
            // Calculate item total
            // -------------------------------------------------
            var totalPrice =
                item.Quantity * item.UnitPrice;

            totalAmount += totalPrice;

            bill.Items.Add(new BillItem
            {
                MedicineId = item.MedicineId,

                Quantity = item.Quantity,

                UnitPrice = item.UnitPrice,

                TotalPrice = totalPrice
            });
        }

        // -----------------------------------------------------
        // 7. Calculate Bill Amounts
        // -----------------------------------------------------
        bill.TotalAmount = totalAmount;

        if (bill.PaidAmount > bill.TotalAmount)
        {
            throw new InvalidOperationException(
                "Paid amount cannot be greater than total bill amount.");
        }

        bill.BalanceAmount =
            bill.TotalAmount - bill.PaidAmount;

        // -----------------------------------------------------
        // 8. Save Bill
        // -----------------------------------------------------
        _context.Bills.Add(bill);

        await _context.SaveChangesAsync();

        // -----------------------------------------------------
        // 9. Create Audit Log
        // -----------------------------------------------------
        await _auditLogService.LogAsync(
            action: "Create",
            entityName: "Bill",
            entityId: bill.Id,
            description:
                $"Bill '{bill.BillNumber}' created for patient " +
                $"'{patient.Name}' with total amount {bill.TotalAmount}, " +
                $"paid amount {bill.PaidAmount}, " +
                $"balance amount {bill.BalanceAmount}."
        );

        // -----------------------------------------------------
        // 10. Load Created Bill
        // -----------------------------------------------------
        var createdBill = await _context.Bills
            .AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.Dispensing)
            .Include(x => x.Items)
                .ThenInclude(x => x.Medicine)
            .FirstAsync(x => x.Id == bill.Id);

        return MapToDto(createdBill);
    }

    // =========================================================
    // GENERATE BILL NUMBER
    // =========================================================
    private async Task<string> GenerateBillNumberAsync()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");

        var prefix = $"BILL-{datePart}-";

        var lastBillNumber = await _context.Bills
            .Where(x => x.BillNumber.StartsWith(prefix))
            .OrderByDescending(x => x.Id)
            .Select(x => x.BillNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;

        if (!string.IsNullOrWhiteSpace(lastBillNumber))
        {
            var numberPart = lastBillNumber
                .Replace(prefix, "");

            if (int.TryParse(numberPart, out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D3}";
    }

    // =========================================================
    // MAP ENTITY TO DTO
    // =========================================================
    private static BillDto MapToDto(Bill bill)
    {
        return new BillDto
        {
            Id = bill.Id,

            PatientId = bill.PatientId,

            DispensingId = bill.DispensingId,

            BillNumber = bill.BillNumber,

            BillDate = bill.BillDate,

            TotalAmount = bill.TotalAmount,

            PaidAmount = bill.PaidAmount,

            BalanceAmount = bill.BalanceAmount,

            CreatedAt = bill.CreatedAt,

            Items = bill.Items
                .Select(item => new BillItemDto
                {
                    Id = item.Id,

                    MedicineId = item.MedicineId,

                    MedicineName = item.Medicine.Name,

                    Quantity = item.Quantity,

                    UnitPrice = item.UnitPrice,

                    TotalPrice = item.TotalPrice
                })
                .ToList()
        };
    }
}
//using BKMC.Pharmacy.Application.DTOs.Billing;
//using BKMC.Pharmacy.Application.Interfaces;
//using BKMC.Pharmacy.Domain.Entities;
//using BKMC.Pharmacy.Infrastructure.Data;
//using Microsoft.EntityFrameworkCore;

//namespace BKMC.Pharmacy.Application.Services;

//public class BillService : IBillService
//{
//    private readonly ApplicationDbContext _context;

//    public BillService(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    // =========================================================
//    // GET ALL BILLS
//    // =========================================================
//    public async Task<IEnumerable<BillDto>> GetAllAsync()
//    {
//        var bills = await _context.Bills
//            .AsNoTracking()
//            .Include(x => x.Patient)
//            .Include(x => x.Dispensing)
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Medicine)
//            .OrderByDescending(x => x.BillDate)
//            .ToListAsync();

//        return bills.Select(MapToDto);
//    }

//    // =========================================================
//    // GET BILL BY ID
//    // =========================================================
//    public async Task<BillDto?> GetByIdAsync(int id)
//    {
//        var bill = await _context.Bills
//            .AsNoTracking()
//            .Include(x => x.Patient)
//            .Include(x => x.Dispensing)
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Medicine)
//            .FirstOrDefaultAsync(x => x.Id == id);

//        if (bill == null)
//        {
//            return null;
//        }

//        return MapToDto(bill);
//    }

//    // =========================================================
//    // CREATE BILL
//    // =========================================================
//    public async Task<BillDto> CreateAsync(CreateBillDto dto)
//    {
//        // -----------------------------------------------------
//        // 1. Validate Patient
//        // -----------------------------------------------------
//        var patient = await _context.Patients
//            .FirstOrDefaultAsync(x => x.Id == dto.PatientId);

//        if (patient == null)
//        {
//            throw new InvalidOperationException(
//                "Patient not found.");
//        }

//        // -----------------------------------------------------
//        // 2. Validate Bill Items
//        // -----------------------------------------------------
//        if (dto.Items == null || dto.Items.Count == 0)
//        {
//            throw new InvalidOperationException(
//                "At least one bill item is required.");
//        }

//        // -----------------------------------------------------
//        // 3. Validate Paid Amount
//        // -----------------------------------------------------
//        if (dto.PaidAmount < 0)
//        {
//            throw new InvalidOperationException(
//                "Paid amount cannot be negative.");
//        }

//        // -----------------------------------------------------
//        // 4. Validate Dispensing
//        // -----------------------------------------------------
//        if (dto.DispensingId.HasValue)
//        {
//            var dispensing = await _context.Dispensings
//                .FirstOrDefaultAsync(x =>
//                    x.Id == dto.DispensingId.Value);

//            if (dispensing == null)
//            {
//                throw new InvalidOperationException(
//                    "Dispensing record not found.");
//            }

//            if (dispensing.PatientId != dto.PatientId)
//            {
//                throw new InvalidOperationException(
//                    "The dispensing record does not belong to the selected patient.");
//            }
//        }

//        // -----------------------------------------------------
//        // 5. Create Bill
//        // -----------------------------------------------------
//        var bill = new Bill
//        {
//            PatientId = dto.PatientId,

//            DispensingId = dto.DispensingId,

//            BillNumber = await GenerateBillNumberAsync(),

//            BillDate = dto.BillDate,

//            PaidAmount = dto.PaidAmount,

//            CreatedAt = DateTime.UtcNow
//        };

//        decimal totalAmount = 0;

//        // -----------------------------------------------------
//        // 6. Create Bill Items
//        // -----------------------------------------------------
//        foreach (var item in dto.Items)
//        {
//            // Quantity validation
//            if (item.Quantity <= 0)
//            {
//                throw new InvalidOperationException(
//                    "Bill item quantity must be greater than zero.");
//            }

//            // Unit price validation
//            if (item.UnitPrice < 0)
//            {
//                throw new InvalidOperationException(
//                    "Unit price cannot be negative.");
//            }

//            // Medicine validation
//            var medicine = await _context.Medicines
//                .FirstOrDefaultAsync(x =>
//                    x.Id == item.MedicineId &&
//                    x.IsActive);

//            if (medicine == null)
//            {
//                throw new InvalidOperationException(
//                    $"Medicine with ID {item.MedicineId} " +
//                    "not found or inactive.");
//            }

//            // -------------------------------------------------
//            // Calculate item total
//            // -------------------------------------------------
//            var totalPrice =
//                item.Quantity * item.UnitPrice;

//            totalAmount += totalPrice;

//            bill.Items.Add(new BillItem
//            {
//                MedicineId = item.MedicineId,

//                Quantity = item.Quantity,

//                UnitPrice = item.UnitPrice,

//                TotalPrice = totalPrice
//            });
//        }

//        // -----------------------------------------------------
//        // 7. Calculate Bill Amounts
//        // -----------------------------------------------------
//        bill.TotalAmount = totalAmount;

//        if (bill.PaidAmount > bill.TotalAmount)
//        {
//            throw new InvalidOperationException(
//                "Paid amount cannot be greater than total bill amount.");
//        }

//        bill.BalanceAmount =
//            bill.TotalAmount - bill.PaidAmount;

//        // -----------------------------------------------------
//        // 8. Save Bill
//        // -----------------------------------------------------
//        _context.Bills.Add(bill);

//        await _context.SaveChangesAsync();

//        // -----------------------------------------------------
//        // 9. Load Created Bill
//        // -----------------------------------------------------
//        var createdBill = await _context.Bills
//            .AsNoTracking()
//            .Include(x => x.Patient)
//            .Include(x => x.Dispensing)
//            .Include(x => x.Items)
//                .ThenInclude(x => x.Medicine)
//            .FirstAsync(x => x.Id == bill.Id);

//        return MapToDto(createdBill);
//    }

//    // =========================================================
//    // GENERATE BILL NUMBER
//    // =========================================================
//    private async Task<string> GenerateBillNumberAsync()
//    {
//        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");

//        var prefix = $"BILL-{datePart}-";

//        var lastBillNumber = await _context.Bills
//            .Where(x => x.BillNumber.StartsWith(prefix))
//            .OrderByDescending(x => x.Id)
//            .Select(x => x.BillNumber)
//            .FirstOrDefaultAsync();

//        int nextNumber = 1;

//        if (!string.IsNullOrWhiteSpace(lastBillNumber))
//        {
//            var numberPart = lastBillNumber
//                .Replace(prefix, "");

//            if (int.TryParse(numberPart, out var lastNumber))
//            {
//                nextNumber = lastNumber + 1;
//            }
//        }

//        return $"{prefix}{nextNumber:D3}";
//    }

//    // =========================================================
//    // MAP ENTITY TO DTO
//    // =========================================================
//    private static BillDto MapToDto(Bill bill)
//    {
//        return new BillDto
//        {
//            Id = bill.Id,

//            PatientId = bill.PatientId,

//            DispensingId = bill.DispensingId,

//            BillNumber = bill.BillNumber,

//            BillDate = bill.BillDate,

//            TotalAmount = bill.TotalAmount,

//            PaidAmount = bill.PaidAmount,

//            BalanceAmount = bill.BalanceAmount,

//            CreatedAt = bill.CreatedAt,

//            Items = bill.Items
//                .Select(item => new BillItemDto
//                {
//                    Id = item.Id,

//                    MedicineId = item.MedicineId,

//                    MedicineName = item.Medicine.Name,

//                    Quantity = item.Quantity,

//                    UnitPrice = item.UnitPrice,

//                    TotalPrice = item.TotalPrice
//                })
//                .ToList()
//        };
//    }
//}

