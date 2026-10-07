using BKMC.Pharmacy.Web.Models.Receiving;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Receiving;

public class IndexModel : PageModel
{
    private readonly ReceivingApiService _receivingApiService;

    public IndexModel(
        ReceivingApiService receivingApiService)
    {
        _receivingApiService = receivingApiService;
    }

    public List<ReceivingDto> Receivings { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            Receivings =
                await _receivingApiService.GetAllAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Unable to load receiving records. {ex.Message}";

            Receivings = new List<ReceivingDto>();
        }
    }
}