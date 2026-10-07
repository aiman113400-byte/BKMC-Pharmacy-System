using BKMC.Pharmacy.Web.Models.Billing;
using BKMC.Pharmacy.Web.Models.Dispensings;
using BKMC.Pharmacy.Web.Models.Medicines;
using BKMC.Pharmacy.Web.Models.Patients;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Bills;

public class CreateModel : PageModel
{
    private readonly BillApiService _billApiService;
    private readonly PatientApiService _patientApiService;
    private readonly MedicineApiService _medicineApiService;
    private readonly DispensingApiService _dispensingApiService;

    public CreateModel(
        BillApiService billApiService,
        PatientApiService patientApiService,
        MedicineApiService medicineApiService,
        DispensingApiService dispensingApiService)
    {
        _billApiService = billApiService;
        _patientApiService = patientApiService;
        _medicineApiService = medicineApiService;
        _dispensingApiService = dispensingApiService;
    }

    [BindProperty]
    public CreateBillModel Bill { get; set; } = new();

    public List<PatientDto> Patients { get; set; } = new();

    public List<MedicineDto> Medicines { get; set; } = new();

    public List<DispensingDto> Dispensings { get; set; } = new();

    public async Task OnGetAsync()
    {
        Bill.BillDate = DateTime.Now;

        await LoadPatientsAsync();
        await LoadMedicinesAsync();
        await LoadDispensingsAsync();

        if (Bill.Items.Count == 0)
        {
            Bill.Items.Add(new CreateBillItemModel());
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Bill.PatientId <= 0)
        {
            ModelState.AddModelError(
                "Bill.PatientId",
                "Please select a patient.");
        }

        if (Bill.Items == null || Bill.Items.Count == 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "At least one bill item is required.");
        }

        if (!ModelState.IsValid)
        {
            await LoadPatientsAsync();
            await LoadMedicinesAsync();
            await LoadDispensingsAsync();

            return Page();
        }

        try
        {
            var result =
                await _billApiService.CreateAsync(Bill);

            if (result == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Bill API returned no bill.");

                await LoadPatientsAsync();
                await LoadMedicinesAsync();
                await LoadDispensingsAsync();

                return Page();
            }

            TempData["SuccessMessage"] =
                $"Bill {result.BillNumber} created successfully.";

            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                string.Empty,
                "Bill creation failed: " + ex.Message);

            await LoadPatientsAsync();
            await LoadMedicinesAsync();
            await LoadDispensingsAsync();

            return Page();
        }
    }

    private async Task LoadPatientsAsync()
    {
        Patients =
            await _patientApiService.GetAllAsync();
    }

    private async Task LoadMedicinesAsync()
    {
        Medicines =
            await _medicineApiService.GetAllAsync(
                isActive: true);
    }

    private async Task LoadDispensingsAsync()
    {
        Dispensings =
            await _dispensingApiService.GetAllAsync();
    }
}





//using BKMC.Pharmacy.Web.Models.Billing;
//using BKMC.Pharmacy.Web.Models.Dispensings;
//using BKMC.Pharmacy.Web.Models.Medicines;
//using BKMC.Pharmacy.Web.Models.Patients;
//using BKMC.Pharmacy.Web.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//namespace BKMC.Pharmacy.Web.Pages.Bills;

//public class CreateModel : PageModel
//{
//    private readonly BillApiService _billApiService;
//    private readonly PatientApiService _patientApiService;
//    private readonly MedicineApiService _medicineApiService;
//    private readonly DispensingApiService _dispensingApiService;

//    public CreateModel(
//        BillApiService billApiService,
//        PatientApiService patientApiService,
//        MedicineApiService medicineApiService,
//        DispensingApiService dispensingApiService)
//    {
//        _billApiService = billApiService;
//        _patientApiService = patientApiService;
//        _medicineApiService = medicineApiService;
//        _dispensingApiService = dispensingApiService;
//    }

//    [BindProperty]
//    public CreateBillModel Bill { get; set; } = new();

//    public PatientDto? Patient { get; set; }

//    public List<MedicineDto> Medicines { get; set; } = new();

//    public List<DispensingDto> Dispensings { get; set; } = new();

//    public async Task OnGetAsync()
//    {
//        Bill.BillDate = DateTime.Now;

//        await LoadMedicinesAsync();
//        await LoadDispensingsAsync();
//    }

//    public async Task<IActionResult> OnPostLookupPatientAsync(
//        string mrNumber)
//    {
//        if (string.IsNullOrWhiteSpace(mrNumber))
//        {
//            ModelState.Clear();

//            await LoadMedicinesAsync();
//            await LoadDispensingsAsync();

//            return Page();
//        }

//        Patient =
//            await _patientApiService
//                .GetByMRNumberAsync(mrNumber.Trim());

//        if (Patient == null)
//        {
//            ModelState.AddModelError(
//                "MRNumber",
//                "Patient not found.");

//            await LoadMedicinesAsync();
//            await LoadDispensingsAsync();

//            return Page();
//        }

//        Bill.PatientId = Patient.Id;

//        if (Bill.BillDate == default)
//        {
//            Bill.BillDate = DateTime.Now;
//        }

//        await LoadMedicinesAsync();
//        await LoadDispensingsAsync();

//        return Page();
//    }

//    public async Task<IActionResult> OnPostAsync()
//    {
//        if (Bill.PatientId <= 0)
//        {
//            ModelState.AddModelError(
//                string.Empty,
//                "Please lookup a patient first.");
//        }

//        if (Bill.Items == null || Bill.Items.Count == 0)
//        {
//            ModelState.AddModelError(
//                string.Empty,
//                "At least one bill item is required.");
//        }

//        if (!ModelState.IsValid)
//        {
//            var validationErrors = ModelState
//                .Where(x => x.Value?.Errors.Count > 0)
//                .SelectMany(x => x.Value!.Errors)
//                .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage)
//                    ? x.Exception?.Message
//                    : x.ErrorMessage)
//                .Where(x => !string.IsNullOrWhiteSpace(x))
//                .ToList();

//            ModelState.AddModelError(
//                string.Empty,
//                "Bill form validation failed: " +
//                string.Join(" | ", validationErrors));

//            await LoadPatientAsync();
//            await LoadMedicinesAsync();
//            await LoadDispensingsAsync();

//            return Page();
//        }

//        try
//        {
//            var result =
//                await _billApiService.CreateAsync(Bill);

//            if (result == null)
//            {
//                ModelState.AddModelError(
//                    string.Empty,
//                    "Bill API returned no bill.");

//                await LoadPatientAsync();
//                await LoadMedicinesAsync();
//                await LoadDispensingsAsync();

//                return Page();
//            }

//            return RedirectToPage("./Index");
//        }
//        catch (Exception ex)
//        {
//            ModelState.AddModelError(
//                string.Empty,
//                "Bill creation failed: " + ex.Message);

//            await LoadPatientAsync();
//            await LoadMedicinesAsync();
//            await LoadDispensingsAsync();

//            return Page();
//        }
//    }

//    private async Task LoadPatientAsync()
//    {
//        if (Bill.PatientId <= 0)
//        {
//            Patient = null;
//            return;
//        }

//        Patient =
//            await _patientApiService
//                .GetByIdAsync(Bill.PatientId);
//    }

//    private async Task LoadMedicinesAsync()
//    {
//        Medicines =
//            await _medicineApiService
//                .GetAllAsync(isActive: true);
//    }

//    private async Task LoadDispensingsAsync()
//    {
//        Dispensings =
//            await _dispensingApiService
//                .GetAllAsync();
//    }
//}