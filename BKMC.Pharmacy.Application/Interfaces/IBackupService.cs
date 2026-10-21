using BKMC.Pharmacy.Application.DTOs;
using BKMC.Pharmacy.Domain.Entities;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IBackupService
{
    Task<string> CreateBackupAsync();
    Task<IEnumerable<BackupHistoryDto>> GetBackupHistoryAsync();
}