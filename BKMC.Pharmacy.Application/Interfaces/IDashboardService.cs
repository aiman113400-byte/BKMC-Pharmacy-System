using BKMC.Pharmacy.Application.DTOs.Dashboard;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync();
}