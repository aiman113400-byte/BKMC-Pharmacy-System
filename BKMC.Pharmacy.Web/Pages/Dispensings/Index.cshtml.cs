using BKMC.Pharmacy.Web.Models.Dispensings;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Dispensings;

public class IndexModel : PageModel
{
    private readonly DispensingApiService _dispensingApiService;

    public IndexModel(DispensingApiService dispensingApiService)
    {
        _dispensingApiService = dispensingApiService;
    }

    public List<DispensingDto> Dispensings { get; set; }
        = new();

    public async Task OnGetAsync()
    {
        Dispensings =
            await _dispensingApiService.GetAllAsync();
    }
}