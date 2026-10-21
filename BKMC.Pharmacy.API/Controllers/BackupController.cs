
using BKMC.Pharmacy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BKMC.Pharmacy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BackupController : ControllerBase
{
    private readonly IBackupService _backupService;
    private readonly ILogger<BackupController> _logger;

    public BackupController(
        IBackupService backupService,
        ILogger<BackupController> logger)
    {
        _backupService = backupService;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateBackup()
    {
        try
        {
            var backupPath =
                await _backupService.CreateBackupAsync();

            return Ok(new
            {
                message = "Database backup created successfully.",
                fileName = Path.GetFileName(backupPath),
                backupPath
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create BKMC Pharmacy database backup.");

            return StatusCode(500, new
            {
                message =
                    "Backup failed. Check API logs and SQL Server permissions."
            });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetBackupHistory()
    {
        try
        {
            var history =
                await _backupService.GetBackupHistoryAsync();

            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve backup history.");

            return StatusCode(500, new
            {
                message =
                    "Unable to retrieve backup history."
            });
        }
    }
}
