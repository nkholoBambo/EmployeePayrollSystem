public class Employee
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public Departments Department { get; set; }

    public string Position { get; set; } = string.Empty;

    public decimal Salary { get; set; }

    public DateTime HireDate { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public int? GetAge(DateTime asOfDate)
    {
        // 1. If DateOfBirth is not set, return null
        if (!DateOfBirth.HasValue)
        {
            return null;
        }

        // 2. Extract the birth date value safely
        DateTime dob = DateOfBirth.Value;

        // 3. Calculate age based on the provided asOfDate rather than today
        int age = asOfDate.Year - dob.Year;

        // 4. Adjust if their birthday hasn't occurred yet by that specific date
        if (asOfDate.Month < dob.Month || (asOfDate.Month == dob.Month && asOfDate.Day < dob.Day))
        {
            age--;
        }

        return age;
    }


    public static DateTime GetTaxYearEnd(int payMonth, int payYear)
    {
        // If the pay month is between March (3) and December (12), 
        // the tax year ends in February of the next year.
        if (payMonth >= 3)
        {
            return new DateTime(payYear + 1, 2, 28);
        }

        // If the pay month is January (1) or February (2), 
        // the tax year ends in February of the current year.
        return new DateTime(payYear, 2, 28);
    }


    public decimal GetTaxRebate(int? age)
    {
        if (!age.HasValue)
        {
            return 0m;
        }


        if (age >= 75)
        {
            return 30_834m;
        }

        if (age >= 65)
        {
            return 27_585m;
        }


        return 17_820m;
    }

    public decimal GetAnnualTaxBeforeRebate(decimal annualTaxableIncome)
    {
        // 1. Handle zero or negative income first
        if (annualTaxableIncome <= 0m)
        {
            return 0m;
        }

        // 2. Check from highest income bracket to lowest
        if (annualTaxableIncome >= 1878600m)
        {
            return 666339m + (annualTaxableIncome - 1878600m) * 0.45m;
        }

        if (annualTaxableIncome >= 887000m)
        {
            return 259783m + (annualTaxableIncome - 887000m) * 0.41m;
        }

        if (annualTaxableIncome >= 695800m)
        {
            return 185215m + (annualTaxableIncome - 695800m) * 0.39m;
        }

        if (annualTaxableIncome >= 530200m)
        {
            return 125599m + (annualTaxableIncome - 530200m) * 0.36m;
        }

        if (annualTaxableIncome >= 383100m)
        {
            return 79998m + (annualTaxableIncome - 383100m) * 0.31m;
        }

        if (annualTaxableIncome >= 245100m)
        {
            return 44118m + (annualTaxableIncome - 245100m) * 0.26m;
        }

        // 3. Lowest bracket: R0 to R245,100 is simply 18% of the income
        return annualTaxableIncome * 0.18m;
    }


    public decimal GetAnnualTaxAfterRebate(
    decimal annualTaxableIncome,
    int? age)
    {
        decimal finalTaxRebate = GetAnnualTaxBeforeRebate(annualTaxableIncome) - GetTaxRebate(age);

        return Math.Max(0m, finalTaxRebate);
    }

    public decimal CalculateMonthlyPAYE(
    decimal monthlySalary,
    int? age)
    {
        if (monthlySalary < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(monthlySalary), "Monthly salary cannot be negative.");
        }

        decimal annualTaxableIncome = monthlySalary * 12m;
        decimal annualTax = GetAnnualTaxAfterRebate(annualTaxableIncome, age);
        decimal monthlyPAYE = Math.Round(annualTax / 12m, 2, MidpointRounding.AwayFromZero);
        return monthlyPAYE;
    }


}

/*
Id
FirstName
LastName
Email
Department
Position
Salary
HireDate
*/