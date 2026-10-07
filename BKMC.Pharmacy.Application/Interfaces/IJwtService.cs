using BKMC.Pharmacy.Domain.Entities;

namespace BKMC.Pharmacy.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(AdminUser adminUser);

    DateTime GetExpirationTime();
}