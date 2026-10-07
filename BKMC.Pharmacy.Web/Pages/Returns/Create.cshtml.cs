using BKMC.Pharmacy.Web.Models.Dispensings;
using BKMC.Pharmacy.Web.Models.Patients;
using BKMC.Pharmacy.Web.Models.Returns;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Returns;

public class CreateModel : PageModel
{
    private readonly ReturnApiService _returnApiService;
    private readonly PatientApiService _patientApiService;
    private readonly DispensingApiService _dispensingApiService;

    public CreateModel(
        ReturnApiService returnApiService,
        PatientApiService patientApiService,
        DispensingApiService dispensingApiService)
    {
        _returnApiService = returnApiService;
        _patientApiService = patientApiService;
        _dispensingApiService = dispensingApiService;
    }

    [BindProperty]
    public CreateReturnModel Return { get; set; } = new();

    public List<PatientDto> Patients { get; set; } = new();

    public List<DispensingDto> Dispensings { get; set; } = new();

    public async Task OnGetAsync()
    {
        Return.ReturnDate = DateTime.Now;

        Patients = await _patientApiService.GetAllAsync();
        Dispensings = await _dispensingApiService.GetAllAsync();

        Console.WriteLine($"Patients loaded: {Patients.Count}");
        Console.WriteLine($"Dispensings loaded: {Dispensings.Count}");
    }
 


    public async Task<IActionResult> OnPostAsync()
    {
        if (Return.PatientId <= 0)
        {
            ModelState.AddModelError(
                "Return.PatientId",
                "Please select a patient.");
        }

        if (!Return.DispensingId.HasValue ||
            Return.DispensingId.Value <= 0)
        {
            ModelState.AddModelError(
                "Return.DispensingId",
                "Please select a dispensing record.");
        }

        if (Return.Items == null ||
            Return.Items.Count == 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "At least one return item is required.");
        }

        if (!ModelState.IsValid)
        {
            Patients = await _patientApiService.GetAllAsync();
            Dispensings = await _dispensingApiService.GetAllAsync();

            return Page();
        }

        try
        {
            var result =
                await _returnApiService.CreateAsync(Return);

            if (result == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Return could not be created.");

                Patients = await _patientApiService.GetAllAsync();
                Dispensings = await _dispensingApiService.GetAllAsync();

                return Page();
            }

            TempData["SuccessMessage"] =
                "Return created successfully.";

            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                string.Empty,
                $"Return creation failed: {ex.Message}");

            Patients = await _patientApiService.GetAllAsync();
            Dispensings = await _dispensingApiService.GetAllAsync();

            return Page();
        }
    }

    public async Task<JsonResult> OnGetDispensingItemsAsync(int id)
    {
        var dispensing =
            await _dispensingApiService.GetByIdAsync(id);

        if (dispensing == null)
        {
            return new JsonResult(new
            {
                items = new List<object>()
            });
        }

        var items = dispensing.Items
            .Select(item => new
            {
                id = item.Id,
                medicineName = item.MedicineName,
                batchNumber = item.BatchNumber,
                quantity = item.Quantity
            })
            .ToList();

        return new JsonResult(new
        {
            items
        });
    }


}