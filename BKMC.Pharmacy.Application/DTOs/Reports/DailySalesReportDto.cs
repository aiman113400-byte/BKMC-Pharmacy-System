namespace BKMC.Pharmacy.Application.DTOs.Reports;

public class DailySalesReportDto
{
    public DateTime Date { get; set; }

    public int TotalBills { get; set; }

    public decimal GrossSales { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public decimal MedicinesSold { get; set; }

    public List<DailySalesBillDto> Bills { get; set; } = new();
}

public class DailySalesBillDto
{
    public int BillId { get; set; }

    public string BillNumber { get; set; } = string.Empty;

    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public DateTime BillDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public decimal MedicinesSold { get; set; }
}