using BKMC.Pharmacy.Application.DTOs.StockTransfers;
using BKMC.Pharmacy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BKMC.Pharmacy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StockTransfersController : ControllerBase
{
    private readonly IStockTransferService _stockTransferService;

    public StockTransfersController(
        IStockTransferService stockTransferService)
    {
        _stockTransferService = stockTransferService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var transfers =
            await _stockTransferService.GetAllAsync();

        return Ok(transfers);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var transfer =
            await _stockTransferService.GetByIdAsync(id);

        if (transfer == null)
        {
            return NotFound(new
            {
                message = "Stock transfer not found."
            });
        }

        return Ok(transfer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateStockTransferDto dto)
    {
        try
        {
            var transfer =
                await _stockTransferService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = transfer.Id },
                transfer);
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