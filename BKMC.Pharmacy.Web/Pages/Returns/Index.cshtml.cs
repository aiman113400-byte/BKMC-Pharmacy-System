using BKMC.Pharmacy.Web.Models.Returns;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Returns;

public class IndexModel : PageModel
{
    private readonly ReturnApiService _returnApiService;

    public IndexModel(
        ReturnApiService returnApiService)
    {
        _returnApiService = returnApiService;
    }

    public List<ReturnDto> Returns { get; set; }
        = new();

    public async Task OnGetAsync()
    {
        Returns =
            await _returnApiService.GetAllAsync();
    }
}