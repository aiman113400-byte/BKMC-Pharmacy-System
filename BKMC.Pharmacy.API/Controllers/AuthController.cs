using BKMC.Pharmacy.Application.DTOs.Auth;
using BKMC.Pharmacy.Application.Interfaces;
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

        var response = new LoginResponseDto
        {
            Token = token,
            Username = admin.Username,
            Role = "Admin",
            ExpiresAt = expiresAt
        };

        return Ok(response);
    }
}