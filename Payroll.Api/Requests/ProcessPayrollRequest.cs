using System.ComponentModel.DataAnnotations;

namespace Payroll.Api.Requests;

public class ProcessPayrollRequest
{
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Range(1, 12)]
    public int Month { get; set; }

    [Range(2000, 2100)]
    public int Year { get; set; }

    [Range(0, 10000000)]
    public decimal OtherDeductions { get; set; }
}