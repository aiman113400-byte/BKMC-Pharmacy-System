using BKMC.Pharmacy.Application.DTOs.Return;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IReturnService
{
    Task<IEnumerable<ReturnDto>> GetAllAsync();

    Task<ReturnDto?> GetByIdAsync(int id);

    Task<ReturnDto> CreateAsync(CreateReturnDto dto);
}