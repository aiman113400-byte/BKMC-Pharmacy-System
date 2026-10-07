using BKMC.Pharmacy.Web.Models.Billing;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Bills;

public class IndexModel : PageModel
{
    private readonly BillApiService _billApiService;

    public IndexModel(BillApiService billApiService)
    {
        _billApiService = billApiService;
    }

    public List<BillDto> Bills { get; set; } = new List<BillDto>();

    public async Task OnGetAsync()
    {
        Bills = await _billApiService.GetAllAsync();
    }
}