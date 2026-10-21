using Azure.Storage.Blobs;
using BKMC.Pharmacy.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BKMC.Pharmacy.Infrastructure.Services;

public class AzureBlobBackupService : ICloudBackupService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureBlobBackupService> _logger;

    public AzureBlobBackupService(
        IConfiguration configuration,
        ILogger<AzureBlobBackupService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task UploadBackupAsync(string backupFilePath)
    {
        if (string.IsNullOrWhiteSpace(backupFilePath))
        {
            throw new ArgumentException(
                "Backup file path is required.",
                nameof(backupFilePath));
        }

        if (!File.Exists(backupFilePath))
        {
            throw new FileNotFoundException(
                "Backup file was not found.",
                backupFilePath);
        }

        var enabled =
            _configuration.GetValue<bool>(
                "CloudBackupSettings:Enabled");

        if (!enabled)
        {
            _logger.LogInformation(
                "Cloud backup is disabled.");

            return;
        }

        var connectionString =
            _configuration["CloudBackupSettings:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Cloud backup connection string is not configured.");
        }

        var containerName =
            _configuration["CloudBackupSettings:ContainerName"];

        if (string.IsNullOrWhiteSpace(containerName))
        {
            throw new InvalidOperationException(
                "Cloud backup container name is not configured.");
        }

        var blobServiceClient =
            new BlobServiceClient(connectionString);

        var containerClient =
            blobServiceClient.GetBlobContainerClient(containerName);

        await containerClient.CreateIfNotExistsAsync();

        var fileName =
            Path.GetFileName(backupFilePath);

        var blobClient =
            containerClient.GetBlobClient(fileName);

        await using var fileStream =
            File.OpenRead(backupFilePath);

        await blobClient.UploadAsync(
            fileStream,
            overwrite: true);

        _logger.LogInformation(
            "Backup uploaded successfully to Azure Blob Storage: {FileName}",
            fileName);
    }
}