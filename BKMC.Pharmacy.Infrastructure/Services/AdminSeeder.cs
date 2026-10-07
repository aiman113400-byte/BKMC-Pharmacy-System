using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Domain.Enums;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.Infrastructure.Services;

public class AdminSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public AdminSeeder(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        var adminExists = await _context.AdminUsers
            .AnyAsync(x => x.Role == (int)UserRole.Admin);

        if (adminExists)
        {
            return;
        }

        var admin = new AdminUser
        {
            Username = "admin",
            PasswordHash = _passwordHasher.HashPassword("Admin@123"),
            Role = (int)UserRole.Admin,
            IsActive = true
        };

        _context.AdminUsers.Add(admin);

        await _context.SaveChangesAsync();
    }
}