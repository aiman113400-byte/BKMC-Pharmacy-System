using BKMC.Pharmacy.Application.DTOs.Suppliers;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly ApplicationDbContext _context;

    public SupplierService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SupplierDto>> GetAllAsync()
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Select(s => MapToDto(s))
            .ToListAsync();
    }

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        return supplier == null
            ? null
            : MapToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(
        CreateSupplierDto dto)
    {
        var supplierCodeExists =
            await _context.Suppliers.AnyAsync(
                s => s.SupplierCode == dto.SupplierCode);

        if (supplierCodeExists)
        {
            throw new InvalidOperationException(
                "Supplier code already exists.");
        }

        var supplier = new Supplier
        {
            SupplierCode = dto.SupplierCode,
            Name = dto.Name,
            ContactPerson = dto.ContactPerson,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Suppliers.Add(supplier);

        await _context.SaveChangesAsync();

        return MapToDto(supplier);
    }

    public async Task<SupplierDto?> UpdateAsync(
        int id,
        UpdateSupplierDto dto)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
            return null;

        var duplicateCode =
            await _context.Suppliers.AnyAsync(
                s =>
                    s.SupplierCode == dto.SupplierCode &&
                    s.Id != id);

        if (duplicateCode)
        {
            throw new InvalidOperationException(
                "Supplier code already exists.");
        }

        supplier.SupplierCode = dto.SupplierCode;
        supplier.Name = dto.Name;
        supplier.ContactPerson = dto.ContactPerson;
        supplier.Phone = dto.Phone;
        supplier.Email = dto.Email;
        supplier.Address = dto.Address;
        supplier.IsActive = dto.IsActive;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(supplier);
    }

    public async Task<bool> ActivateAsync(int id)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
            return false;

        supplier.IsActive = true;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
            return false;

        supplier.IsActive = false;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<SupplierDto>> SearchAsync(
        string? search)
    {
        var query = _context.Suppliers
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(s =>
                s.Name.Contains(search) ||
                s.SupplierCode.Contains(search) ||
                (s.ContactPerson != null &&
                 s.ContactPerson.Contains(search)));
        }

        return await query
            .Select(s => MapToDto(s))
            .ToListAsync();
    }

    private static SupplierDto MapToDto(
        Supplier supplier)
    {
        return new SupplierDto
        {
            Id = supplier.Id,
            SupplierCode = supplier.SupplierCode,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt,
            UpdatedAt = supplier.UpdatedAt
        };
    }
}