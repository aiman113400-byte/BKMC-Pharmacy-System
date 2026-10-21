using BKMC.Pharmacy.Application.DTOs.Auth;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Domain.Enums;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BKMC.Pharmacy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthController(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(
        LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "Username and password are required."
            });
        }

        var admin = await _context.AdminUsers
            .FirstOrDefaultAsync(x =>
                x.Username == request.Username);

        if (admin == null)
        {
            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        if (!admin.IsActive)
        {
            return Unauthorized(new
            {
                message = "Admin account is inactive."
            });
        }

        var passwordValid = _passwordHasher.VerifyPassword(
            request.Password,
            admin.PasswordHash);

        if (!passwordValid)
        {
            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        var token = _jwtService.GenerateToken(admin);
        var expiresAt = _jwtService.GetExpirationTime();

        //var response = new LoginResponseDto
        //{
        //    Token = token,
        //    Username = admin.Username,
        //    Role = "Admin",
        //    ExpiresAt = expiresAt
        //};
        var response = new LoginResponseDto
        {
            Token = token,
            Username = admin.Username,
            Role = ((UserRole)admin.Role).ToString(),
            ExpiresAt = expiresAt
        };

        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "Username and password are required."
            });
        }

        if (request.Role != 1 && request.Role != 2)
        {
            return BadRequest(new
            {
                message = "Invalid role. Allowed roles are User (1) and Admin (2)."
            });
        }

        var usernameExists = await _context.AdminUsers
            .AnyAsync(x => x.Username == request.Username);

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
            Username = request.Username.Trim(),
            PasswordHash = passwordHash,
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.AdminUsers.Add(adminUser);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "User registered successfully.",
            userId = adminUser.Id,
            username = adminUser.Username,
            role = adminUser.Role,
            isActive = adminUser.IsActive
        });
    }

}