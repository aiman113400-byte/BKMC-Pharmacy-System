using BKMC.Pharmacy.Application.DTOs.Receiving;
using BKMC.Pharmacy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BKMC.Pharmacy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReceivingsController : ControllerBase
    {
        private readonly IReceivingService _receivingService;

        public ReceivingsController(
            IReceivingService receivingService)
        {
            _receivingService = receivingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var receivings = await _receivingService.GetAllAsync();

            return Ok(receivings);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var receiving =
                await _receivingService.GetByIdAsync(id);

            if (receiving == null)
                return NotFound(new
                {
                    message = "Receiving not found."
                });

            return Ok(receiving);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateReceivingDto dto)
        {
            try
            {
                var receiving =
                    await _receivingService.CreateAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = receiving.Id },
                    receiving);
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
}
