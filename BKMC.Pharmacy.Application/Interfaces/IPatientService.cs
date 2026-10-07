using BKMC.Pharmacy.Application.DTOs.Patients;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IPatientService
{
    Task<IEnumerable<PatientDto>> GetAllAsync();

    Task<PatientDto?> GetByIdAsync(int id);

    Task<PatientDto?> GetByMRNumberAsync(string mrNumber);

    Task<PatientDto> CreateAsync(CreatePatientDto dto);

    Task<PatientDto?> UpdateAsync(
        int id,
        UpdatePatientDto dto);
}