using BKMC.Pharmacy.Application.DTOs.Suppliers;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface ISupplierService
{
    Task<IEnumerable<SupplierDto>> GetAllAsync();

    Task<SupplierDto?> GetByIdAsync(int id);

    Task<SupplierDto> CreateAsync(CreateSupplierDto dto);

    Task<SupplierDto?> UpdateAsync(
        int id,
        UpdateSupplierDto dto);

    Task<bool> ActivateAsync(int id);

    Task<bool> DeactivateAsync(int id);

    Task<IEnumerable<SupplierDto>> SearchAsync(string? search);
}