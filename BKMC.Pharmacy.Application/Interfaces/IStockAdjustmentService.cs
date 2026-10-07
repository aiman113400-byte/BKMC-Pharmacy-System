using BKMC.Pharmacy.Application.DTOs.StockAdjustments;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IStockAdjustmentService
{
    Task<IEnumerable<StockAdjustmentDto>> GetAllAsync();

    Task<StockAdjustmentDto?> GetByIdAsync(int id);

    Task<StockAdjustmentDto> CreateAsync(
        CreateStockAdjustmentDto dto);
}