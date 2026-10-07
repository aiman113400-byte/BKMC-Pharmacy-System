namespace BKMC.Pharmacy.Application.DTOs.Billing;

public class BillDto
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int? DispensingId { get; set; }

    public string BillNumber { get; set; } = string.Empty;

    public DateTime BillDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<BillItemDto> Items { get; set; }
        = new();
}

public class BillItemDto
{
    public int Id { get; set; }

    public int MedicineId { get; set; }

    public string MedicineName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }
}

public class CreateBillDto
{
    public int PatientId { get; set; }

    public int? DispensingId { get; set; }

    public DateTime BillDate { get; set; }

    public decimal PaidAmount { get; set; }

    public List<CreateBillItemDto> Items { get; set; }
        = new();
}

public class CreateBillItemDto
{
    public int MedicineId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}