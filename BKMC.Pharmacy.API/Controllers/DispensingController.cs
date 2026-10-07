using BKMC.Pharmacy.Application.DTOs.Dispensing;
using BKMC.Pharmacy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BKMC.Pharmacy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DispensingsController : ControllerBase
{
    private readonly IDispensingService _dispensingService;

    public DispensingsController(
        IDispensingService dispensingService)
    {
        _dispensingService = dispensingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var dispensings =
            await _dispensingService.GetAllAsync();

        return Ok(dispensings);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dispensing =
            await _dispensingService.GetByIdAsync(id);

        if (dispensing == null)
        {
            return NotFound(new
            {
                message = "Dispensing record not found."
            });
        }

        return Ok(dispensing);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateDispensingDto dto)
    {
        try
        {
            var dispensing =
                await _dispensingService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = dispensing.Id },
                dispensing);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}