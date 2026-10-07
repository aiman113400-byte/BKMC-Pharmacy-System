using BKMC.Pharmacy.Application.DTOs.Billing;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IBillService
{
    Task<IEnumerable<BillDto>> GetAllAsync();

    Task<BillDto?> GetByIdAsync(int id);

    Task<BillDto> CreateAsync(CreateBillDto dto);
}