using BKMC.Pharmacy.Application.DTOs.StockTransactions;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IStockTransactionService
{
    Task<IEnumerable<StockTransactionDto>> GetAllAsync();

    Task<StockTransactionDto?> GetByIdAsync(int id);

    Task<StockTransactionDto> CreateAsync(
        CreateStockTransactionDto dto);
}