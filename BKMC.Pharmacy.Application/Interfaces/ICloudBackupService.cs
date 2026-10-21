namespace BKMC.Pharmacy.Application.Interfaces;

public interface ICloudBackupService
{
    Task UploadBackupAsync(string backupFilePath);
}