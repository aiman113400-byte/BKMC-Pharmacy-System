using BKMC.Pharmacy.Application.DTOs;
using BKMC.Pharmacy.Application.DTOs.Medicines;
using System;
using System.Collections.Generic;
using System.Text;

namespace BKMC.Pharmacy.Application.Interfaces
{
    
public interface IMedicineService
{
    Task<MedicineDto> CreateAsync(CreateMedicineDto dto);
        Task<List<MedicineDto>> GetAllAsync(
        string? search = null,
        bool? isActive = null);

        Task<MedicineDto?> GetByIdAsync(int id);

        Task<MedicineDto?> UpdateAsync(
            int id,
            UpdateMedicineDto dto);

        Task<bool> ActivateAsync(int id);

        Task<bool> DeactivateAsync(int id);
    }
}
