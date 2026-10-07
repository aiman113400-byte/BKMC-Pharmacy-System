using BKMC.Pharmacy.Application.DTOs.Dispensing;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IDispensingService
{
    Task<IEnumerable<DispensingDto>> GetAllAsync();

    Task<DispensingDto?> GetByIdAsync(int id);

    Task<DispensingDto> CreateAsync(
        CreateDispensingDto dto);
}