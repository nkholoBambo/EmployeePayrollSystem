using Xunit;

namespace Payroll.Tests;

public class PayeTests
{
    // CalculateMonthlyPAYE and friends don't use any employee fields,
    // so one blank Employee is enough for the pure tax-math tests.
    private static readonly Employee TaxMath = new Employee();

    private static Employee MakeEmployee(decimal salary, DateTime dateOfBirth) => new Employee
    {
        Id = 1,
        FirstName = "Test",
        LastName = "Employee",
        Email = "test@example.com",
        Position = "Developer",
        Salary = salary,
        HireDate = new DateTime(2020, 1, 1),
        DateOfBirth = dateOfBirth
    };

    // ---------- Annual tax before rebate (the bracket table) ----------

    public static TheoryData<decimal, decimal> BracketCases => new()
    {
        { 0m,         0m },
        { -5000m,     0m },          // negative income is treated as zero
        { 100000m,    18000m },      // inside the first bracket: 18%
        { 245099m,    44117.82m },   // R1 below the first boundary
        { 245100m,    44118m },      // exactly on the boundary
        { 245101m,    44118.26m },   // R1 into the 26% bracket
        { 383100m,    79998m },
        { 530200m,    125599m },
        { 695800m,    185215m },
        { 887000m,    259783m },
        { 1878600m,   666339m },
        { 1878601m,   666339.45m },  // R1 into the 45% bracket
    };

    [Theory]
    [MemberData(nameof(BracketCases))]
    public void AnnualTaxBeforeRebate_MatchesSarsTable(decimal income, decimal expectedTax)
    {
        Assert.Equal(expectedTax, TaxMath.GetAnnualTaxBeforeRebate(income));
    }

    // ---------- Rebates ----------

    public static TheoryData<int?, decimal> RebateCases => new()
    {
        { null, 0m },
        { 30,   17820m },
        { 64,   17820m },
        { 65,   27585m },   // primary + secondary
        { 74,   27585m },
        { 75,   30834m },   // primary + secondary + tertiary
        { 90,   30834m },
    };

    [Theory]
    [MemberData(nameof(RebateCases))]
    public void TaxRebate_DependsOnAge(int? age, decimal expectedRebate)
    {
        Assert.Equal(expectedRebate, TaxMath.GetTaxRebate(age));
    }

    // ---------- Monthly PAYE (worked by hand from the SARS table) ----------

    public static TheoryData<decimal, int, decimal> MonthlyPayeCases => new()
    {
        // salary, age, expected monthly PAYE
        { 8000m,   30, 0m },          // under the R99,000 a year threshold
        { 8250m,   30, 0m },          // exactly on the threshold
        { 8251m,   30, 0.18m },       // R1 over the threshold
        { 10000m,  30, 315.00m },
        { 20000m,  30, 2115.00m },
        { 30000m,  30, 4681.00m },
        { 50000m,  30, 11075.58m },   // rounds down from 11,075.5833
        { 100000m, 30, 30857.75m },
        { 200000m, 30, 73595.75m },   // top bracket (45%)
        { 30000m,  64, 4681.00m },    // one year before the secondary rebate
        { 30000m,  65, 3867.25m },
        { 30000m,  74, 3867.25m },
        { 30000m,  75, 3596.50m },
        { 8000m,   75, 0m },          // rebate bigger than the tax: never negative
    };

    [Theory]
    [MemberData(nameof(MonthlyPayeCases))]
    public void MonthlyPaye_MatchesHandCalculation(decimal salary, int age, decimal expectedPaye)
    {
        Assert.Equal(expectedPaye, TaxMath.CalculateMonthlyPAYE(salary, age));
    }

    [Fact]
    public void MonthlyPaye_WithUnknownAge_GetsNoRebate()
    {
        // 10,000 x 12 = 120,000 x 18% = 21,600 a year / 12 = 1,800 a month
        Assert.Equal(1800.00m, TaxMath.CalculateMonthlyPAYE(10000m, null));
    }

    [Fact]
    public void MonthlyPaye_NegativeSalary_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TaxMath.CalculateMonthlyPAYE(-1m, 30));
    }

    // ---------- Age and tax year end ----------

    [Theory]
    [InlineData(3, 2026)]
    [InlineData(12, 2026)]
    [InlineData(1, 2027)]
    [InlineData(2, 2027)]
    public void TaxYearEnd_IsEndOfFebruary2027_ForEveryMonthOfThe2026TaxYear(int month, int year)
    {
        Assert.Equal(new DateTime(2027, 2, 28), Employee.GetTaxYearEnd(month, year));
    }

    [Fact]
    public void GetAge_BirthdayNotYetReached_IsOneYearYounger()
    {
        var employee = MakeEmployee(20000m, new DateTime(1990, 6, 15));

        Assert.Equal(35, employee.GetAge(new DateTime(2026, 6, 14)));
        Assert.Equal(36, employee.GetAge(new DateTime(2026, 6, 15)));
    }

    [Fact]
    public void GetAge_WithoutDateOfBirth_IsNull()
    {
        var employee = MakeEmployee(20000m, DateTime.Today);
        employee.DateOfBirth = null;

        Assert.Null(employee.GetAge(new DateTime(2027, 2, 28)));
    }

    // ---------- Whole payslip, start to finish ----------

    [Fact]
    public void Payslip_R30000_Under65_NoOtherDeductions()
    {
        var employee = MakeEmployee(30000m, new DateTime(1990, 1, 1));

        var record = PayrollCalculator.CalculatePayroll(employee, 0m, 3, 2026);

        Assert.Equal(30000m, record.GrossPay);
        Assert.Equal(4681.00m, record.PAYE);
        Assert.Equal(177.12m, record.UIF);       // capped at 1% of R17,712
        Assert.Equal(25141.88m, record.NetPay);  // 30,000 - 4,681.00 - 177.12
    }

    [Fact]
    public void Payslip_OtherDeductionsReduceNetPay()
    {
        var employee = MakeEmployee(30000m, new DateTime(1990, 1, 1));

        var record = PayrollCalculator.CalculatePayroll(employee, 500m, 6, 2026);

        Assert.Equal(24641.88m, record.NetPay);  // 25,141.88 - 500
    }

    [Fact]
    public void Payslip_TurnsSixtyFiveOnTaxYearEndDay_GetsSecondaryRebate()
    {
        // Born 28 Feb 1962: turns 65 on 28 Feb 2027, the last day of the tax year.
        var employee = MakeEmployee(30000m, new DateTime(1962, 2, 28));

        var record = PayrollCalculator.CalculatePayroll(employee, 0m, 3, 2026);

        Assert.Equal(3867.25m, record.PAYE);
    }

    [Fact]
    public void Payslip_TurnsSixtyFiveOneDayAfterTaxYearEnd_GetsNoSecondaryRebate()
    {
        // Born 1 Mar 1962: still 64 on 28 Feb 2027.
        var employee = MakeEmployee(30000m, new DateTime(1962, 3, 1));

        var record = PayrollCalculator.CalculatePayroll(employee, 0m, 3, 2026);

        Assert.Equal(4681.00m, record.PAYE);
    }
}