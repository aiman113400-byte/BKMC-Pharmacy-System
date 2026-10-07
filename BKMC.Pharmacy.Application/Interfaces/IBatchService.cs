using BKMC.Pharmacy.Application.DTOs.Batches;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IBatchService
{
    Task<IEnumerable<BatchDto>> GetAllAsync();

    Task<BatchDto?> GetByIdAsync(int id);

    Task<IEnumerable<BatchLookupDto>> GetByMedicineIdAsync(int medicineId);

    Task<IEnumerable<BatchLookupDto>> GetAvailableBatchesAsync(int medicineId);
}