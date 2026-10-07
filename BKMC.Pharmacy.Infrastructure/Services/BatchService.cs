using BKMC.Pharmacy.Application.DTOs.Batches;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Infrastructure.Services;

public class BatchService : IBatchService
{
    private readonly ApplicationDbContext _context;

    public BatchService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BatchDto>> GetAllAsync()
    {
        return await _context.Batches
            .Include(b => b.Medicine)
            .Select(b => new BatchDto
            {
                Id = b.Id,
                MedicineId = b.MedicineId,
                MedicineName = b.Medicine.Name,
                BatchNumber = b.BatchNumber,
                ManufacturingDate = b.ManufacturingDate,
                ExpiryDate = b.ExpiryDate,
                Quantity = b.Quantity,
                CreatedAt = b.CreatedAt
            })
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }

    public async Task<BatchDto?> GetByIdAsync(int id)
    {
        return await _context.Batches
            .Include(b => b.Medicine)
            .Where(b => b.Id == id)
            .Select(b => new BatchDto
            {
                Id = b.Id,
                MedicineId = b.MedicineId,
                MedicineName = b.Medicine.Name,
                BatchNumber = b.BatchNumber,
                ManufacturingDate = b.ManufacturingDate,
                ExpiryDate = b.ExpiryDate,
                Quantity = b.Quantity,
                CreatedAt = b.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<BatchLookupDto>> GetByMedicineIdAsync(
        int medicineId)
    {
        return await _context.Batches
            .Where(b => b.MedicineId == medicineId)
            .Select(b => new BatchLookupDto
            {
                Id = b.Id,
                MedicineId = b.MedicineId,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate,
                Quantity = b.Quantity,
                IsExpired = b.ExpiryDate.Date < DateTime.UtcNow.Date
            })
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BatchLookupDto>> GetAvailableBatchesAsync(
        int medicineId)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.Batches
            .Where(b =>
                b.MedicineId == medicineId &&
                b.Quantity > 0 &&
                b.ExpiryDate.Date >= today)
            .Select(b => new BatchLookupDto
            {
                Id = b.Id,
                MedicineId = b.MedicineId,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate,
                Quantity = b.Quantity,
                IsExpired = false
            })
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }
}