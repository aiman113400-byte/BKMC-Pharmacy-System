using BKMC.Pharmacy.Application.DTOs.StockTransfers;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IStockTransferService
{
    Task<IEnumerable<StockTransferDto>> GetAllAsync();

    Task<StockTransferDto?> GetByIdAsync(int id);

    Task<StockTransferDto> CreateAsync(
        CreateStockTransferDto dto);
}