using BKMC.Pharmacy.Application.DTOs.Suppliers;
using BKMC.Pharmacy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BKMC.Pharmacy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(
        ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var suppliers =
            await _supplierService.GetAllAsync();

        return Ok(suppliers);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var supplier =
            await _supplierService.GetByIdAsync(id);

        if (supplier == null)
        {
            return NotFound(new
            {
                message = "Supplier not found."
            });
        }

        return Ok(supplier);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? search)
    {
        var suppliers =
            await _supplierService.SearchAsync(search);

        return Ok(suppliers);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateSupplierDto dto)
    {
        try
        {
            var supplier =
                await _supplierService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = supplier.Id },
                supplier);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateSupplierDto dto)
    {
        try
        {
            var supplier =
                await _supplierService.UpdateAsync(id, dto);

            if (supplier == null)
            {
                return NotFound(new
                {
                    message = "Supplier not found."
                });
            }

            return Ok(supplier);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        var result =
            await _supplierService.ActivateAsync(id);

        if (!result)
        {
            return NotFound(new
            {
                message = "Supplier not found."
            });
        }

        return Ok(new
        {
            message = "Supplier activated successfully."
        });
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result =
            await _supplierService.DeactivateAsync(id);

        if (!result)
        {
            return NotFound(new
            {
                message = "Supplier not found."
            });
        }

        return Ok(new
        {
            message = "Supplier deactivated successfully."
        });
    }
}