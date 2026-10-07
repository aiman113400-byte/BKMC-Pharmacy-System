using BKMC.Pharmacy.Application.DTOs.Receiving;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IReceivingService
{
    Task<IEnumerable<ReceivingDto>> GetAllAsync();

    Task<ReceivingDto?> GetByIdAsync(int id);

    Task<ReceivingDto> CreateAsync(CreateReceivingDto dto);
}