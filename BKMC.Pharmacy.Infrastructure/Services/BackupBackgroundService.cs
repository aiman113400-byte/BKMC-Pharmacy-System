using BKMC.Pharmacy.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BKMC.Pharmacy.Infrastructure.Services;

public class BackupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackupBackgroundService> _logger;

    public BackupBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<BackupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "BKMC automatic backup scheduler started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var enabled =
                    _configuration.GetValue<bool>(
                        "BackupSettings:AutomaticBackupEnabled");

                if (!enabled)
                {
                    await Task.Delay(
                        TimeSpan.FromMinutes(1),
                        stoppingToken);

                    continue;
                }

                var backupTimeValue =
                    _configuration["BackupSettings:BackupTime"];

                if (!TimeSpan.TryParse(
                        backupTimeValue,
                        out var backupTime))
                {
                    _logger.LogWarning(
                        "Invalid BackupSettings:BackupTime value.");

                    await Task.Delay(
                        TimeSpan.FromMinutes(1),
                        stoppingToken);

                    continue;
                }

                var now = DateTime.Now;

                var nextBackup =
                    now.Date.Add(backupTime);

                if (nextBackup <= now)
                {
                    nextBackup =
                        nextBackup.AddDays(1);
                }

                var delay =
                    nextBackup - now;

                _logger.LogInformation(
                    "Next automatic database backup scheduled for {BackupTime}.",
                    nextBackup);

                await Task.Delay(
                    delay,
                    stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                // Re-check setting in case automatic backup
                // was disabled while waiting.
                var stillEnabled =
                    _configuration.GetValue<bool>(
                        "BackupSettings:AutomaticBackupEnabled");

                if (!stillEnabled)
                {
                    continue;
                }

                using var scope =
                    _scopeFactory.CreateScope();

                var backupService =
                    scope.ServiceProvider
                        .GetRequiredService<IBackupService>();

                _logger.LogInformation(
                    "Starting automatic BKMC database backup.");

                var backupPath =
                    await backupService.CreateBackupAsync();

                _logger.LogInformation(
                    "Automatic BKMC database backup completed: {BackupPath}",
                    backupPath);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Automatic BKMC database backup failed.");

                // Prevent continuous rapid retries if an unexpected
                // scheduler error occurs.
                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);
            }
        }

        _logger.LogInformation(
            "BKMC automatic backup scheduler stopped.");
    }
}