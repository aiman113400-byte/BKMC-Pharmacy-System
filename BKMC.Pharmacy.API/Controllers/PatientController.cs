using BKMC.Pharmacy.Application.DTOs.Patients;
using BKMC.Pharmacy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BKMC.Pharmacy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(
        IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var patients =
            await _patientService.GetAllAsync();

        return Ok(patients);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var patient =
            await _patientService.GetByIdAsync(id);

        if (patient == null)
        {
            return NotFound(new
            {
                message = "Patient not found."
            });
        }

        return Ok(patient);
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> GetByMRNumber(
        [FromQuery] string mrNumber)
    {
        var patient =
            await _patientService
                .GetByMRNumberAsync(mrNumber);

        if (patient == null)
        {
            return NotFound(new
            {
                message = "Patient with this MR Number was not found."
            });
        }

        return Ok(patient);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreatePatientDto dto)
    {
        try
        {
            var patient =
                await _patientService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = patient.Id },
                patient);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdatePatientDto dto)
    {
        try
        {
            var patient =
                await _patientService.UpdateAsync(id, dto);

            if (patient == null)
            {
                return NotFound(new
                {
                    message = "Patient not found."
                });
            }

            return Ok(patient);
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