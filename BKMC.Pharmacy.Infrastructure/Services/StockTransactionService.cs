using BKMC.Pharmacy.Application.DTOs.StockTransactions;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Infrastructure.Services;

public class StockTransactionService : IStockTransactionService
{
    private readonly ApplicationDbContext _context;

    public StockTransactionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockTransactionDto>> GetAllAsync()
    {
        return await _context.StockTransactions
            .AsNoTracking()
            .Select(x => new StockTransactionDto
            {
                Id = x.Id,
                MedicineId = x.MedicineId,
                BatchId = x.BatchId,
                Location = x.Location,
                TransactionType = x.TransactionType,
                Quantity = x.Quantity,
                BalanceQuantity = x.BalanceQuantity,
                ReferenceNumber = x.ReferenceNumber,
                TransactionDate = x.TransactionDate
            })
            .ToListAsync();
    }

    public async Task<StockTransactionDto?> GetByIdAsync(int id)
    {
        return await _context.StockTransactions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new StockTransactionDto
            {
                Id = x.Id,
                MedicineId = x.MedicineId,
                BatchId = x.BatchId,
                Location = x.Location,
                TransactionType = x.TransactionType,
                Quantity = x.Quantity,
                BalanceQuantity = x.BalanceQuantity,
                ReferenceNumber = x.ReferenceNumber,
                TransactionDate = x.TransactionDate
            })
            .FirstOrDefaultAsync();
    }

    public async Task<StockTransactionDto> CreateAsync(
        CreateStockTransactionDto dto)
    {
        var transaction = new StockTransaction
        {
            MedicineId = dto.MedicineId,
            BatchId = dto.BatchId,
            Location = dto.Location,
            TransactionType = dto.TransactionType,
            Quantity = dto.Quantity,
            BalanceQuantity = dto.BalanceQuantity,
            ReferenceNumber = dto.ReferenceNumber,
            TransactionDate = dto.TransactionDate == default
                ? DateTime.UtcNow
                : dto.TransactionDate
        };

        _context.StockTransactions.Add(transaction);

        await _context.SaveChangesAsync();

        return new StockTransactionDto
        {
            Id = transaction.Id,
            MedicineId = transaction.MedicineId,
            BatchId = transaction.BatchId,
            Location = transaction.Location,
            TransactionType = transaction.TransactionType,
            Quantity = transaction.Quantity,
            BalanceQuantity = transaction.BalanceQuantity,
            ReferenceNumber = transaction.ReferenceNumber,
            TransactionDate = transaction.TransactionDate
        };
    }
}