using BKMC.Pharmacy.Application.DTOs.Stock;
using BKMC.Pharmacy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BKMC.Pharmacy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StocksController : ControllerBase
{
    private readonly IStockService _stockService;

    public StocksController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var stocks = await _stockService.GetAllAsync();

        return Ok(stocks);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var stock = await _stockService.GetByIdAsync(id);

        if (stock == null)
        {
            return NotFound(new
            {
                message = "Stock not found."
            });
        }

        return Ok(stock);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateStockDto dto)
    {
        try
        {
            var stock = await _stockService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = stock.Id },
                stock);
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