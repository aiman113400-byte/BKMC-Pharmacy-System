using BKMC.Pharmacy.Application.DTOs.StockAdjustments;
using BKMC.Pharmacy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BKMC.Pharmacy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StockAdjustmentsController : ControllerBase
{
    private readonly IStockAdjustmentService _service;

    public StockAdjustmentsController(
        IStockAdjustmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var adjustments = await _service.GetAllAsync();

        return Ok(adjustments);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var adjustment = await _service.GetByIdAsync(id);

        if (adjustment == null)
        {
            return NotFound();
        }

        return Ok(adjustment);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateStockAdjustmentDto dto)
    {
        try
        {
            var adjustment = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = adjustment.Id },
                adjustment);
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