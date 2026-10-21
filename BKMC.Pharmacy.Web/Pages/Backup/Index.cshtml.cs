using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Backup;

public class IndexModel : PageModel
{
    private readonly BackupApiService _backupApiService;

    public IndexModel(BackupApiService backupApiService)
    {
        _backupApiService = backupApiService;
    }

    public List<BackupHistoryDto> BackupHistory { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        BackupHistory =
            await _backupApiService.GetHistoryAsync();
    }

    public async Task<IActionResult> OnPostCreateBackupAsync()
    {
        var result =
            await _backupApiService.CreateBackupAsync();

        if (result == null)
        {
            ErrorMessage =
                "Backup failed. Please check the API logs.";

            return RedirectToPage();
        }

        SuccessMessage =
            $"Backup created successfully: {result.FileName}";

        return RedirectToPage();
    }
}