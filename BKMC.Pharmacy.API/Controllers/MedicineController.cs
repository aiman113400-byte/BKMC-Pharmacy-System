using BKMC.Pharmacy.Application.DTOs.Medicines;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BKMC.Pharmacy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class MedicinesController : ControllerBase
{
    private readonly IMedicineService _medicineService;
    private readonly ApplicationDbContext _context;
    public MedicinesController(IMedicineService medicineService, ApplicationDbContext context)
    {
        _medicineService = medicineService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var medicines = await _medicineService.GetAllAsync(
            search,
            isActive);

        return Ok(medicines);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var medicine = await _medicineService.GetByIdAsync(id);

        if (medicine == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Medicine not found.",
                data = (object?)null
            });
        }

        return Ok(new
        {
            success = true,
            message = "Medicine retrieved successfully.",
            data = medicine
        });
    }

    //[HttpGet("select")]
    //public async Task<IActionResult> SelectPatients()
    //{
    //    var patients = await _context.Patients.ToListAsync();

    //    return Ok(patients);
    //}

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateMedicineDto dto)
    {
        try
        {
            var medicine = await _medicineService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = medicine.Id },
                new
                {
                    success = true,
                    message = "Medicine created successfully.",
                    data = medicine
                });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                success = false,
                message = ex.Message,
                data = (object?)null
            });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMedicineDto dto)
    {
        var medicine = await _medicineService.UpdateAsync(id, dto);

        if (medicine == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Medicine not found.",
                data = (object?)null
            });
        }

        return Ok(new
        {
            success = true,
            message = "Medicine updated successfully.",
            data = medicine
        });
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        var result = await _medicineService.ActivateAsync(id);

        if (!result)
        {
            return NotFound(new
            {
                success = false,
                message = "Medicine not found.",
                data = (object?)null
            });
        }

        return Ok(new
        {
            success = true,
            message = "Medicine activated successfully.",
            data = (object?)null
        });
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _medicineService.DeactivateAsync(id);

        if (!result)
        {
            return NotFound(new
            {
                success = false,
                message = "Medicine not found.",
                data = (object?)null
            });
        }

        return Ok(new
        {
            success = true,
            message = "Medicine deactivated successfully.",
            data = (object?)null
        });
    }
}