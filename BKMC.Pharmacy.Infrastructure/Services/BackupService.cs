
using System.Security.Claims;
using BKMC.Pharmacy.Application.Interfaces;
using BKMC.Pharmacy.Domain.Entities;
using BKMC.Pharmacy.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BKMC.Pharmacy.Application.DTOs;

namespace BKMC.Pharmacy.Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<BackupService> _logger;

    public BackupService(
        IConfiguration configuration,
        ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<BackupService> logger)
    {
        _configuration = configuration;
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<string> CreateBackupAsync()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"BKMCPharmacyDb_{timestamp}.bak";
        string backupPath = string.Empty;

        try
        {
            var connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string is not configured.");
            }

            var backupDirectory =
                _configuration["BackupSettings:Directory"];

            if (string.IsNullOrWhiteSpace(backupDirectory))
            {
                throw new InvalidOperationException(
                    "Backup directory is not configured.");
            }

            var connectionBuilder =
                new SqlConnectionStringBuilder(connectionString);

            var databaseName = connectionBuilder.InitialCatalog;

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException(
                    "Database name is missing from the connection string.");
            }

            if (new[] { "master", "model", "msdb", "tempdb" }
                .Contains(databaseName, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "System databases cannot be backed up by this service.");
            }

            Directory.CreateDirectory(backupDirectory);

            fileName = $"{databaseName}_{timestamp}.bak";

            backupPath = Path.GetFullPath(
                Path.Combine(backupDirectory, fileName));

            var safeDatabaseName = databaseName.Replace("]", "]]");
            var safeBackupPath = backupPath.Replace("'", "''");

            var sql = $"""
                BACKUP DATABASE [{safeDatabaseName}]
                TO DISK = N'{safeBackupPath}'
                WITH COPY_ONLY, CHECKSUM, INIT, STATS = 10;
                """;

            await using var connection =
                new SqlConnection(connectionString);

            await connection.OpenAsync();

            await using var command =
                new SqlCommand(sql, connection);

            command.CommandTimeout = 0;

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            await TrySaveHistoryAsync(
                fileName,
                "Failed",
                ex.GetType().Name);

            _logger.LogError(
                ex,
                "BKMC Pharmacy database backup failed.");

            throw;
        }

        // The database backup succeeded. History failure must not
        // incorrectly report the actual backup operation as failed.
        await TrySaveHistoryAsync(fileName, "Success", null);

        return backupPath;
    }

    public async Task<IEnumerable<BackupHistoryDto>> GetBackupHistoryAsync()
    {
        return await _dbContext.BackupHistories
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new BackupHistoryDto
            {
                Id = x.Id,
                FileName = x.FileName,
                CreatedAt = x.CreatedAt,
                Status = x.Status,
                CreatedBy = x.CreatedBy,
                ErrorMessage = x.ErrorMessage
            })
            .ToListAsync();
    }

    private async Task TrySaveHistoryAsync(
        string fileName,
        string status,
        string? errorMessage)
    {
        var history = new BackupHistory
        {
            FileName = fileName,
            CreatedAt = DateTime.UtcNow,
            Status = status,
            CreatedBy = GetCurrentUsername(),
            ErrorMessage = errorMessage
        };

        try
        {
            _dbContext.BackupHistories.Add(history);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Prevent a failed insert from remaining tracked.
            _dbContext.Entry(history).State = EntityState.Detached;

            _logger.LogError(
                ex,
                "Could not save backup history for {FileName}.",
                fileName);
        }
    }

    private string GetCurrentUsername()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            return "System";
        }

        return user.Identity.Name
            ?? user.FindFirst(ClaimTypes.Name)?.Value
            ?? user.FindFirst("unique_name")?.Value
            ?? user.FindFirst("name")?.Value
            ?? "System";
    }
}
