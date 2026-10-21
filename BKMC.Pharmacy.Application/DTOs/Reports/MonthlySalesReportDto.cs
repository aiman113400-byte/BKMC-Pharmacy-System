namespace BKMC.Pharmacy.Application.DTOs.Reports;

public class MonthlySalesReportDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public int TotalBills { get; set; }

    public decimal GrossSales { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public decimal MedicinesSold { get; set; }

    public List<MonthlySalesDayDto> DailySales { get; set; } = new();
}

public class MonthlySalesDayDto
{
    public DateTime Date { get; set; }

    public int TotalBills { get; set; }

    public decimal GrossSales { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public decimal MedicinesSold { get; set; }
}