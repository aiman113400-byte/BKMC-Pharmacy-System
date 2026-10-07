using BKMC.Pharmacy.Application.DTOs.Stock;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Application.Services;

public class StockService : IStockService
{
    private readonly ApplicationDbContext _context;

    public StockService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockDto>> GetAllAsync()
    {
        return await _context.Stocks
            .AsNoTracking()
            .Include(x => x.Medicine)
            .Include(x => x.Batch)
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<StockDto?> GetByIdAsync(int id)
    {
        var stock = await _context.Stocks
            .AsNoTracking()
            .Include(x => x.Medicine)
            .Include(x => x.Batch)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (stock == null)
            return null;

        return MapToDto(stock);
    }

    public async Task<StockDto> CreateAsync(CreateStockDto dto)
    {
        if (dto.Quantity <= 0)
        {
            throw new InvalidOperationException(
                "Stock quantity must be greater than zero.");
        }

        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(x =>
                x.Id == dto.MedicineId &&
                x.IsActive);

        if (medicine == null)
        {
            throw new InvalidOperationException(
                "Medicine not found or inactive.");
        }

        var batch = await _context.Batches
            .FirstOrDefaultAsync(x =>
                x.Id == dto.BatchId &&
                x.MedicineId == dto.MedicineId);

        if (batch == null)
        {
            throw new InvalidOperationException(
                "Batch not found or does not belong to the selected medicine.");
        }

        if (batch.ExpiryDate.Date <= DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException(
                "Cannot add expired batch to stock.");
        }

        var existingStock = await _context.Stocks
            .FirstOrDefaultAsync(x =>
                x.MedicineId == dto.MedicineId &&
                x.BatchId == dto.BatchId &&
                x.Location == dto.Location);

        if (existingStock != null)
        {
            existingStock.Quantity += dto.Quantity;
            existingStock.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var stock = new Stock
            {
                MedicineId = dto.MedicineId,
                BatchId = dto.BatchId,
                Location = dto.Location,
                Quantity = dto.Quantity,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Stocks.Add(stock);
        }

        await _context.SaveChangesAsync();

        var result = await _context.Stocks
            .AsNoTracking()
            .Include(x => x.Medicine)
            .Include(x => x.Batch)
            .FirstAsync(x =>
                x.MedicineId == dto.MedicineId &&
                x.BatchId == dto.BatchId &&
                x.Location == dto.Location);

        return MapToDto(result);
    }

    private static StockDto MapToDto(Stock stock)
    {
        return new StockDto
        {
            Id = stock.Id,
            MedicineId = stock.MedicineId,
            MedicineName = stock.Medicine.Name,
            BatchId = stock.BatchId,
            BatchNumber = stock.Batch.BatchNumber,
            Location = stock.Location,
            Quantity = stock.Quantity,
            UpdatedAt = stock.UpdatedAt
        };
    }
}