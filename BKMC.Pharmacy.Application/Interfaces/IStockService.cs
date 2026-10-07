using BKMC.Pharmacy.Application.DTOs.Stock;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IStockService
{
    Task<IEnumerable<StockDto>> GetAllAsync();

    Task<StockDto?> GetByIdAsync(int id);

    Task<StockDto> CreateAsync(CreateStockDto dto);
}