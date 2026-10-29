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

        int payPeriod = payYear * 100 + payMonth;
        if (payPeriod < 202603 || payPeriod > 202702)
        {
            throw new ArgumentOutOfRangeException(nameof(payMonth), "Payroll can only be processed from March 2026 to February 2027, because the tax tables in this system are for the 2026/27 tax year.");
        }

        if (employee.Salary <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(employee.Salary), "Salary must be greater than zero.");
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