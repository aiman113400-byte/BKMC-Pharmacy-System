using BKMC.Pharmacy.Application.DTOs.Admin;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Domain.Enums;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;

namespace BKMC.Pharmacy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
   
    private readonly IPasswordHasher _passwordHasher;

    public AdminController(
      ApplicationDbContext context,
      IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserListDto>>> GetUsers()
    {
        var users = await _context.AdminUsers
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new UserListDto
            {
                Id = x.Id,
                Username = x.Username,
                Role = ((UserRole)x.Role).ToString(),
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(CreateUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new
            {
                message = "Username is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "Password is required."
            });
        }

        if (request.Role != 1 && request.Role != 2)
        {
            return BadRequest(new
            {
                message = "Invalid role. Allowed roles are User (1) and Admin (2)."
            });
        }

        var username = request.Username.Trim();

        var usernameExists = await _context.AdminUsers
            .AnyAsync(x => x.Username == username);

        if (usernameExists)
        {
            return Conflict(new
            {
                message = "Username already exists."
            });
        }

        var passwordHash = _passwordHasher.HashPassword(
            request.Password);

        var adminUser = new AdminUser
        {
            Username = username,
            PasswordHash = passwordHash,
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.AdminUsers.Add(adminUser);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "User created successfully.",
            id = adminUser.Id,
            username = adminUser.Username,
            role = ((UserRole)adminUser.Role).ToString(),
            isActive = adminUser.IsActive,
            createdAt = adminUser.CreatedAt
        });
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(
    int id,
    UpdateUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new
            {
                message = "Username is required."
            });
        }

        if (request.Role != 1 && request.Role != 2)
        {
            return BadRequest(new
            {
                message = "Invalid role. Allowed roles are User (1) and Admin (2)."
            });
        }

        var user = await _context.AdminUsers
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        var username = request.Username.Trim();

        var usernameExists = await _context.AdminUsers
            .AnyAsync(x =>
                x.Username == username &&
                x.Id != id);

        if (usernameExists)
        {
            return Conflict(new
            {
                message = "Username already exists."
            });
        }

        user.Username = username;
        user.Role = request.Role;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "User updated successfully.",
            id = user.Id,
            username = user.Username,
            role = ((UserRole)user.Role).ToString(),
            isActive = user.IsActive,
            createdAt = user.CreatedAt
        });
    }
}