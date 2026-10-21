using BKMC.Pharmacy.Application.DTOs.Medicines;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Infrastructure.Services;

public class MedicineService : IMedicineService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public MedicineService(
        ApplicationDbContext context,
        IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<MedicineDto> CreateAsync(CreateMedicineDto dto)
    {
        var medicineCode = dto.MedicineCode.Trim();

        var exists = await _context.Medicines
            .AnyAsync(x => x.MedicineCode == medicineCode);

        if (exists)
        {
            throw new InvalidOperationException(
                "Medicine code already exists.");
        }

        var medicine = new Medicine
        {
            MedicineCode = medicineCode,
            Name = dto.Name.Trim(),
            GenericName = dto.GenericName.Trim(),
            Strength = dto.Strength.Trim(),
            Form = dto.Form.Trim(),
            ReorderLevel = dto.ReorderLevel,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Medicines.Add(medicine);

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            action: "Create",
            entityName: "Medicine",
            entityId: medicine.Id,
            description: $"Medicine '{medicine.Name}' created."
        );

        return MapToDto(medicine);
    }

    public async Task<List<MedicineDto>> GetAllAsync(
        string? search = null,
        bool? isActive = null)
    {
        var query = _context.Medicines
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchValue = search.Trim();

            query = query.Where(x =>
                x.MedicineCode.Contains(searchValue) ||
                x.Name.Contains(searchValue) ||
                x.GenericName.Contains(searchValue));
        }

        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }

        var medicines = await query
            .OrderBy(x => x.Name)
            .ToListAsync();

        return medicines
            .Select(MapToDto)
            .ToList();
    }

    public async Task<MedicineDto?> GetByIdAsync(int id)
    {
        var medicine = await _context.Medicines
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (medicine == null)
        {
            return null;
        }

        return MapToDto(medicine);
    }

    public async Task<MedicineDto?> UpdateAsync(
        int id,
        UpdateMedicineDto dto)
    {
        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (medicine == null)
        {
            return null;
        }

        medicine.Name = dto.Name.Trim();
        medicine.GenericName = dto.GenericName.Trim();
        medicine.Strength = dto.Strength.Trim();
        medicine.Form = dto.Form.Trim();
        medicine.ReorderLevel = dto.ReorderLevel;
        medicine.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            action: "Update",
            entityName: "Medicine",
            entityId: medicine.Id,
            description: $"Medicine '{medicine.Name}' updated."
        );

        return MapToDto(medicine);
    }

    public async Task<bool> ActivateAsync(int id)
    {
        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (medicine == null)
        {
            return false;
        }

        medicine.IsActive = true;
        medicine.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            action: "Activate",
            entityName: "Medicine",
            entityId: medicine.Id,
            description: $"Medicine '{medicine.Name}' activated."
        );

        return true;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (medicine == null)
        {
            return false;
        }

        medicine.IsActive = false;
        medicine.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            action: "Deactivate",
            entityName: "Medicine",
            entityId: medicine.Id,
            description: $"Medicine '{medicine.Name}' deactivated."
        );

        return true;
    }

    private static MedicineDto MapToDto(Medicine medicine)
    {
        return new MedicineDto
        {
            Id = medicine.Id,
            MedicineCode = medicine.MedicineCode,
            Name = medicine.Name,
            GenericName = medicine.GenericName,
            Strength = medicine.Strength,
            Form = medicine.Form,
            ReorderLevel = medicine.ReorderLevel,
            IsActive = medicine.IsActive,
            CreatedAt = medicine.CreatedAt,
            UpdatedAt = medicine.UpdatedAt
        };
    }
}




//using BKMC.Pharmacy.Application.DTOs.Medicines;
//using BKMC.Pharmacy.Application.Interfaces;
//using BKMC.Pharmacy.Domain.Entities;
//using BKMC.Pharmacy.Infrastructure.Data;
//using Microsoft.EntityFrameworkCore;

//namespace BKMC.Pharmacy.Infrastructure.Services;

//public class MedicineService : IMedicineService
//{
//    private readonly ApplicationDbContext _context;

//    public MedicineService(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<MedicineDto> CreateAsync(CreateMedicineDto dto)
//    {
//        var medicineCode = dto.MedicineCode.Trim();

//        var exists = await _context.Medicines
//            .AnyAsync(x => x.MedicineCode == medicineCode);

//        if (exists)
//        {
//            throw new InvalidOperationException(
//                "Medicine code already exists.");
//        }

//        var medicine = new Medicine
//        {
//            MedicineCode = medicineCode,
//            Name = dto.Name.Trim(),
//            GenericName = dto.GenericName.Trim(),
//            Strength = dto.Strength.Trim(),
//            Form = dto.Form.Trim(),
//            IsActive = true,
//            CreatedAt = DateTime.UtcNow
//        };

//        _context.Medicines.Add(medicine);

//        await _context.SaveChangesAsync();

//        return MapToDto(medicine);
//    }

//    public async Task<List<MedicineDto>> GetAllAsync(
//        string? search = null,
//        bool? isActive = null)
//    {
//        var query = _context.Medicines
//            .AsNoTracking()
//            .AsQueryable();

//        if (!string.IsNullOrWhiteSpace(search))
//        {
//            var searchValue = search.Trim();

//            query = query.Where(x =>
//                x.MedicineCode.Contains(searchValue) ||
//                x.Name.Contains(searchValue) ||
//                x.GenericName.Contains(searchValue));
//        }

//        if (isActive.HasValue)
//        {
//            query = query.Where(x =>
//                x.IsActive == isActive.Value);
//        }

//        var medicines = await query
//            .OrderBy(x => x.Name)
//            .ToListAsync();

//        return medicines
//            .Select(MapToDto)
//            .ToList();
//    }

//    public async Task<MedicineDto?> GetByIdAsync(int id)
//    {
//        var medicine = await _context.Medicines
//            .AsNoTracking()
//            .FirstOrDefaultAsync(x => x.Id == id);

//        if (medicine == null)
//        {
//            return null;
//        }

//        return MapToDto(medicine);
//    }

//    public async Task<MedicineDto?> UpdateAsync(
//        int id,
//        UpdateMedicineDto dto)
//    {
//        var medicine = await _context.Medicines
//            .FirstOrDefaultAsync(x => x.Id == id);

//        if (medicine == null)
//        {
//            return null;
//        }

//        medicine.Name = dto.Name.Trim();
//        medicine.GenericName = dto.GenericName.Trim();
//        medicine.Strength = dto.Strength.Trim();
//        medicine.Form = dto.Form.Trim();
//        medicine.UpdatedAt = DateTime.UtcNow;

//        await _context.SaveChangesAsync();

//        return MapToDto(medicine);
//    }

//    public async Task<bool> ActivateAsync(int id)
//    {
//        var medicine = await _context.Medicines
//            .FirstOrDefaultAsync(x => x.Id == id);

//        if (medicine == null)
//        {
//            return false;
//        }

//        medicine.IsActive = true;
//        medicine.UpdatedAt = DateTime.UtcNow;

//        await _context.SaveChangesAsync();

//        return true;
//    }

//    public async Task<bool> DeactivateAsync(int id)
//    {
//        var medicine = await _context.Medicines
//            .FirstOrDefaultAsync(x => x.Id == id);

//        if (medicine == null)
//        {
//            return false;
//        }

//        medicine.IsActive = false;
//        medicine.UpdatedAt = DateTime.UtcNow;

//        await _context.SaveChangesAsync();

//        return true;
//    }

//    private static MedicineDto MapToDto(Medicine medicine)
//    {
//        return new MedicineDto
//        {
//            Id = medicine.Id,
//            MedicineCode = medicine.MedicineCode,
//            Name = medicine.Name,
//            GenericName = medicine.GenericName,
//            Strength = medicine.Strength,
//            Form = medicine.Form,
//            IsActive = medicine.IsActive,
//            CreatedAt = medicine.CreatedAt,
//            UpdatedAt = medicine.UpdatedAt
//        };
//    }
//}