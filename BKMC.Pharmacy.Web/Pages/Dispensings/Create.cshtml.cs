using BKMC.Pharmacy.Web.Models.Dispensings;
using BKMC.Pharmacy.Web.Models.Medicines;
using BKMC.Pharmacy.Web.Models.Patients;
using BKMC.Pharmacy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BKMC.Pharmacy.Web.Pages.Dispensings;

public class CreateModel : PageModel

{
    private readonly DispensingApiService _dispensingService;
    private readonly PatientApiService _patientService;
    private readonly MedicineApiService _medicineService;

    public CreateModel(
        DispensingApiService dispensingService,
        PatientApiService patientService,
        MedicineApiService medicineService)
    {
        _dispensingService = dispensingService;
        _patientService = patientService;
        _medicineService = medicineService;
    }

    // =========================
    // FORM INPUT
    // =========================



    [BindProperty]
    public CreateDispensingModel Input { get; set; } = new()
    {
        DispensingDate = DateTime.Now,
        Items = new List<CreateDispensingItemModel>()
    };

    // =========================
    // PAGE DATA
    // =========================

    public List<PatientDto> Patients { get; set; }
        = new List<PatientDto>();

    public List<MedicineDto> Medicines { get; set; }
        = new List<MedicineDto>();

    // =========================
    // GET
    // =========================

    public async Task<IActionResult> OnGetAsync()
    {
        Console.WriteLine(">>> CREATE DISPENSING PAGE <<<");

        await LoadPatientsAsync();
        await LoadMedicinesAsync();

        Input.DispensingDate = DateTime.Now;

        if (Input.Items.Count == 0)
        {
            Input.Items.Add(new CreateDispensingItemModel());
        }

        return Page();
    }

    // =========================
    // POST - SAVE DISPENSING
    // =========================

    public async Task<IActionResult> OnPostAsync()
    {
        Console.WriteLine("========================================");
        Console.WriteLine(">>> DISPENSING SAVE STARTED <<<");
        Console.WriteLine("========================================");

        Console.WriteLine(
            ">>> PatientId = " + Input.PatientId);

        Console.WriteLine(
            ">>> DispensingDate = " + Input.DispensingDate);

        Console.WriteLine(
            ">>> PrescriptionNumber = " +
            Input.PrescriptionNumber);

        Console.WriteLine(
            ">>> Remarks = " +
            Input.Remarks);

        Console.WriteLine(
            ">>> Items Count = " +
            (Input.Items?.Count ?? 0));

        if (Input.Items != null)
        {
            foreach (var item in Input.Items)
            {
                Console.WriteLine(
                    $">>> MedicineId={item.MedicineId}, " +
                    $"Quantity={item.Quantity}, " +
                    $"Dosage={item.Dosage}, " +
                    $"BatchId={item.BatchId}");
            }
        }

        // =========================
        // VALIDATION
        // =========================

        if (Input.PatientId <= 0)
        {
            ModelState.AddModelError(
                "Input.PatientId",
                "Please select a patient.");
        }

        if (Input.DispensingDate == default)
        {
            ModelState.AddModelError(
                "Input.DispensingDate",
                "Dispensing date is required.");
        }

        if (Input.Items == null || Input.Items.Count == 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "At least one medicine is required.");
        }
        else
        {
            foreach (var item in Input.Items)
            {
                if (item.MedicineId <= 0)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Please select a medicine.");
                }

                if (item.Quantity <= 0)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Quantity must be greater than zero.");
                }
            }
        }

        Console.WriteLine(
            ">>> ModelState.IsValid = " +
            ModelState.IsValid);

        if (!ModelState.IsValid)
        {
            Console.WriteLine(
                ">>> VALIDATION FAILED");

            await LoadPatientsAsync();
            await LoadMedicinesAsync();

            return Page();
        }

        // =========================
        // CALL BACKEND API
        // =========================

        try
        {
            Console.WriteLine(
                ">>> Calling POST /api/Dispensings ...");

            var result =
                await _dispensingService.CreateAsync(Input);

            if (result == null)
            {
                Console.WriteLine(
                    ">>> API returned NULL");

                ModelState.AddModelError(
                    string.Empty,
                    "Dispensing could not be created.");

                await LoadPatientsAsync();
                await LoadMedicinesAsync();

                return Page();
            }

            Console.WriteLine(
                ">>> DISPENSING CREATED SUCCESSFULLY");

            Console.WriteLine(
                ">>> Dispensing Id = " +
                result.Id);

            TempData["Success"] =
                "Dispensing saved successfully.";

            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "========================================");

            Console.WriteLine(
                ">>> DISPENSING SAVE ERROR");

            Console.WriteLine(ex.ToString());

            Console.WriteLine(
                "========================================");

            ModelState.AddModelError(
                string.Empty,
                "Dispensing save failed: " +
                ex.Message);

            await LoadPatientsAsync();
            await LoadMedicinesAsync();

            return Page();
        }
    }

    // =========================
    // LOAD PATIENTS
    // =========================

    private async Task LoadPatientsAsync()
    {
        try
        {
            Patients =
                await _patientService.GetAllAsync()
                ?? new List<PatientDto>();

            Console.WriteLine(
                ">>> Loaded Patients = " +
                Patients.Count);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                ">>> LoadPatientsAsync ERROR: " +
                ex.Message);

            Patients = new List<PatientDto>();
        }
    }

    // =========================
    // LOAD MEDICINES
    // =========================

    private async Task LoadMedicinesAsync()
    {
        try
        {
            Medicines =
                await _medicineService.GetAllAsync()
                ?? new List<MedicineDto>();

            Console.WriteLine(
                ">>> Loaded Medicines = " +
                Medicines.Count);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                ">>> LoadMedicinesAsync ERROR: " +
                ex.Message);

            Medicines = new List<MedicineDto>();
        }
    }
}



//using BKMC.Pharmacy.Domain.Entities;
//using BKMC.Pharmacy.Web.Models.Batches;
//using BKMC.Pharmacy.Web.Models.Dispensings;
//using BKMC.Pharmacy.Web.Models.Medicines;
//using BKMC.Pharmacy.Web.Models.Patients;
//using BKMC.Pharmacy.Web.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//namespace BKMC.Pharmacy.Web.Pages.Dispensings;

//public class CreateModel : PageModel
//{
//    private readonly PatientApiService _patientApiService;
//    private readonly MedicineApiService _medicineApiService;
//    private readonly BatchApiService _batchApiService;
//    private readonly DispensingApiService _dispensingApiService;

//    public CreateModel(
//        PatientApiService patientApiService,
//        MedicineApiService medicineApiService,
//        BatchApiService batchApiService,
//        DispensingApiService dispensingApiService)
//    {
//        _patientApiService = patientApiService;
//        _medicineApiService = medicineApiService;
//        _batchApiService = batchApiService;
//        _dispensingApiService = dispensingApiService;
//    }

//    [BindProperty]
//    public CreateDispensingModel Dispensing { get; set; } = new();

//    public PatientDto? Patient { get; set; }

//    public List<MedicineDto> Medicines { get; set; } = new();

//    public List<BatchLookupDto> Batches { get; set; } = new();


//    public async Task OnGetAsync()
//    {
//        Dispensing.DispensingDate = DateTime.Now;

//        await LoadMedicinesAsync();
//    }


//    public async Task<IActionResult> OnPostLookupPatientAsync(
//        string mrNumber)

//    {
//        if (string.IsNullOrWhiteSpace(mrNumber))
//        {
//            ModelState.Clear();
//            //ModelState.AddModelError(
//            //    "MRNumber",
//            //    "MR Number is required.");

//            await LoadMedicinesAsync();

//            return Page();
//        }

//        Patient =
//            await _patientApiService.GetByMRNumberAsync(
//                mrNumber.Trim());

//        if (Patient == null)
//        {
//            ModelState.AddModelError(
//                "MRNumber",
//                "Patient not found.");

//            await LoadMedicinesAsync();

//            return Page();
//        }

//        Dispensing.PatientId = Patient.Id;

//        if (Dispensing.DispensingDate == default)
//        {
//            Dispensing.DispensingDate = DateTime.Now;
//        }


//        await LoadMedicinesAsync();

//        return Page();
//    }


//    public async Task<IActionResult> OnGetBatchesAsync(
//        int medicineId)
//    {
//        if (medicineId <= 0)
//        {
//            return new JsonResult(
//                new List<BatchLookupDto>());
//        }

//        var batches =
//            await _batchApiService
//                .GetAvailableBatchesAsync(medicineId);

//        return new JsonResult(batches);
//    }

//    public async Task<IActionResult> OnPostCreateDispensingAsync()
//    {
//        if (Dispensing.PatientId <= 0)
//        {
//            ModelState.AddModelError(string.Empty, "Please lookup a patient first.");
//        }

//        var item = Dispensing.Items?.FirstOrDefault();

//        if (item == null || item.MedicineId <= 0)
//        {
//            ModelState.AddModelError(string.Empty, "Please select a medicine.");
//        }
//        else
//        {
//            if (item.BatchId is null or <= 0)
//                ModelState.AddModelError(string.Empty, "Please select a batch.");

//            if (item.Quantity <= 0)
//                ModelState.AddModelError(string.Empty, "Quantity must be greater than zero.");
//        }

//        if (!ModelState.IsValid)
//        {
//            await LoadPatientAsync();
//            await LoadMedicinesAsync();
//            return Page();
//        }

//        try
//        {
//            var result = await _dispensingApiService.CreateAsync(Dispensing);

//            if (result == null)
//            {
//                ModelState.AddModelError(string.Empty, "Dispensing could not be created.");
//                await LoadPatientAsync();
//                await LoadMedicinesAsync();
//                return Page();
//            }

//            return RedirectToPage("./Index");
//        }
//        catch (Exception ex)
//        {
//            ModelState.AddModelError(string.Empty, ex.Message);
//            await LoadPatientAsync();
//            await LoadMedicinesAsync();
//            return Page();
//        }
//    }

//    //public async Task<IActionResult> OnPostAsync()
//    //{
//    //    if (Dispensing.PatientId <= 0)
//    //    {
//    //        ModelState.AddModelError(
//    //            string.Empty,
//    //            "Please lookup a patient first.");
//    //    }

//    //    if (Dispensing.Items == null ||
//    //        Dispensing.Items.Count == 0)
//    //    {
//    //        ModelState.AddModelError(
//    //            string.Empty,
//    //            "At least one dispensing item is required.");
//    //    }

//    //    if (!ModelState.IsValid)
//    //    {
//    //        await LoadPatientAsync();
//    //        await LoadMedicinesAsync();

//    //        return Page();
//    //    }

//    //    var result =
//    //        await _dispensingApiService
//    //            .CreateAsync(Dispensing);

//    //    if (result == null)
//    //    {
//    //        await LoadPatientAsync();
//    //        await LoadMedicinesAsync();

//    //        ModelState.AddModelError(
//    //            string.Empty,
//    //            "Dispensing could not be created.");

//    //        return Page();
//    //    }

//    //    return RedirectToPage("./Index");
//    //}


//    private async Task LoadPatientAsync()
//    {
//        if (Dispensing.PatientId <= 0)
//        {
//            Patient = null;
//            return;
//        }

//        Patient =
//            await _patientApiService
//                .GetByIdAsync(Dispensing.PatientId);
//    }


//    private async Task LoadMedicinesAsync()
//    {
//        Medicines =
//            await _medicineApiService
//                .GetAllAsync(isActive: true);
//    }
//}