using BKMC.Pharmacy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BKMC.Pharmacy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BatchesController : ControllerBase
{
    private readonly IBatchService _batchService;

    public BatchesController(IBatchService batchService)
    {
        _batchService = batchService;
    }

    // GET: api/batches
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var batches = await _batchService.GetAllAsync();

        return Ok(batches);
    }

    // GET: api/batches/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var batch = await _batchService.GetByIdAsync(id);

        if (batch == null)
        {
            return NotFound(new
            {
                message = "Batch not found."
            });
        }

        return Ok(batch);
    }

    // GET: api/batches/medicine/1
    [HttpGet("medicine/{medicineId:int}")]
    public async Task<IActionResult> GetByMedicineId(int medicineId)
    {
        var batches = await _batchService
            .GetByMedicineIdAsync(medicineId);

        return Ok(batches);
    }

    // GET: api/batches/medicine/1/available
    [HttpGet("medicine/{medicineId:int}/available")]
    public async Task<IActionResult> GetAvailableBatches(int medicineId)
    {
        var batches = await _batchService
            .GetAvailableBatchesAsync(medicineId);

        return Ok(batches);
    }
}