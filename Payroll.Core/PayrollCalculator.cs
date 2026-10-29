public static class PayrollCalculator
{
    public static PayrollRecord CalculatePayroll(Employee? employee, decimal otherDeductions, int payMonth, int payYear)
    {
        if (employee == null)
        {
            throw new ArgumentNullException(nameof(employee), "Employee cannot be null.");
        }

        if (!employee.DateOfBirth.HasValue)
        {
            throw new InvalidOperationException("Employee Date of Birth is required for tax rebate calculations.");
        }

        if (otherDeductions < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(otherDeductions), "Deductions cannot be negative.");
        }

        if (payMonth < 1 || payMonth > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(payMonth), "Pay month must be between 1 and 12.");
        }

        if (payYear < 2026)
        {
            throw new ArgumentOutOfRangeException(nameof(payYear), "Pay year must be 2026 or later.");
        }

        if (employee.Salary <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(employee.Salary), "Salary cannot be negative or eqaute to zero");
        }

        decimal grossPay = employee.Salary;

        DateTime taxYearEnd = Employee.GetTaxYearEnd(payMonth, payYear);
        int? age = employee.GetAge(taxYearEnd);

        decimal paye = employee.CalculateMonthlyPAYE(employee.Salary, age);
        decimal uif = Math.Round(Math.Min(employee.Salary, 17712m) * 0.01m, 2);
        decimal netPay = grossPay - otherDeductions - paye - uif;

        if (netPay < 0m)
        {
            throw new InvalidOperationException("Net pay cannot be negative. Other deductions exceed available earnings.");
        }

        return new PayrollRecord
        {
            EmployeeId = employee.Id,
            PayMonth = payMonth,
            PayYear = payYear,
            GrossPay = grossPay,
            OtherDeductions = otherDeductions,
            PAYE = paye,
            UIF = uif,
            NetPay = netPay,
            ProcessedAt = DateTime.Now
        };
    }
}