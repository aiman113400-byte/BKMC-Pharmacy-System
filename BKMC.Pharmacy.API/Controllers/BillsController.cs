
using BKMC.Pharmacy.Application.DTOs.Billing;
using BKMC.Pharmacy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BKMC.Pharmacy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BillsController : ControllerBase
{
    private readonly IBillService _billService;

    public BillsController(IBillService billService)
    {
        _billService = billService;
    }

    // GET: api/Bills
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var bills = await _billService.GetAllAsync();

        return Ok(bills);
    }

    // GET: api/Bills/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var bill = await _billService.GetByIdAsync(id);

        if (bill == null)
        {
            return NotFound(new
            {
                message = "Bill not found."
            });
        }

        return Ok(bill);
    }

    // POST: api/Bills
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateBillDto dto)
    {
        try
        {
            var bill = await _billService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = bill.Id },
                bill);
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

