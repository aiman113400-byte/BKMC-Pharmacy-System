using BKMC.Pharmacy.Domain.Entities;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(
        string action,
        string entityName,
        int? entityId = null,
        string? userId = null,
        string? description = null);

    Task<List<AuditLog>> GetAllAsync();
}