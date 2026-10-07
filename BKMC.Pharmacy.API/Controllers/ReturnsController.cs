using BKMC.Pharmacy.Application.DTOs.Return;
using BKMC.Pharmacy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



namespace BKMC.Pharmacy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReturnsController : ControllerBase
{
    private readonly IReturnService _returnService;

    public ReturnsController(IReturnService returnService)
    {
        _returnService = returnService;
    }

    // GET: api/Returns
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var returns = await _returnService.GetAllAsync();

        return Ok(returns);
    }

    // GET: api/Returns/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var returnRecord = await _returnService.GetByIdAsync(id);

        if (returnRecord == null)
        {
            return NotFound(new
            {
                message = "Return record not found."
            });
        }

        return Ok(returnRecord);
    }

    // POST: api/Returns
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateReturnDto dto)
    {
        try
        {
            var returnRecord = await _returnService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = returnRecord.Id },
                returnRecord);
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