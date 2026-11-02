using Xunit;

namespace Payroll.Tests;

public class PayrollCalculatorTests
{
    // Helper: builds a valid employee so each test only states what it cares about.
    private static Employee MakeEmployee(decimal salary) => new Employee
    {
        Id = 1,
        FirstName = "Test",
        LastName = "Employee",
        Email = "test@example.com",
        Position = "Developer",
        Salary = salary,
        HireDate = new DateTime(2020, 1, 1),
        DateOfBirth = new DateTime(1990, 1, 1)
    };

    // ---------- UIF ----------

    public static TheoryData<decimal, decimal> UifCases => new()
    {
        { 1000m,  10.00m },   // 1% of a small salary
        { 10000m, 100.00m },  // 1% of a normal salary
        { 17712m, 177.12m },  // exactly on the ceiling
        { 17713m, 177.12m },  // just above the ceiling: capped
        { 50000m, 177.12m },  // well above the ceiling: capped
    };

    [Theory]
    [MemberData(nameof(UifCases))]
    public void Uif_IsOnePercent_CappedAtCeiling(decimal salary, decimal expectedUif)
    {
        var record = PayrollCalculator.CalculatePayroll(MakeEmployee(salary), 0m, 3, 2026);

        Assert.Equal(expectedUif, record.UIF);
    }

    [Fact]
    public void Uif_RoundsHalfUp_ToTheCent()
    {
        // 1% of 10000.50 is exactly 100.005, which is halfway between two cents.
        var record = PayrollCalculator.CalculatePayroll(MakeEmployee(10000.50m), 0m, 3, 2026);

        Assert.Equal(100.01m, record.UIF);
    }

    // ---------- Net pay ----------

    [Fact]
    public void NetPay_EqualsGrossMinusDeductionsPayeAndUif()
    {
        var record = PayrollCalculator.CalculatePayroll(MakeEmployee(30000m), 500m, 6, 2026);

        Assert.Equal(record.GrossPay - record.OtherDeductions - record.PAYE - record.UIF, record.NetPay);
    }

    // ---------- Validation ----------

    [Fact]
    public void NullEmployee_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            PayrollCalculator.CalculatePayroll(null, 0m, 3, 2026));
    }

    [Fact]
    public void MissingDateOfBirth_Throws()
    {
        var employee = MakeEmployee(20000m);
        employee.DateOfBirth = null;

        Assert.Throws<InvalidOperationException>(() =>
            PayrollCalculator.CalculatePayroll(employee, 0m, 3, 2026));
    }

    [Fact]
    public void NegativeDeductions_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PayrollCalculator.CalculatePayroll(MakeEmployee(20000m), -1m, 3, 2026));
    }

    [Fact]
    public void ZeroSalary_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PayrollCalculator.CalculatePayroll(MakeEmployee(0m), 0m, 3, 2026));
    }

    [Fact]
    public void DeductionsLargerThanEarnings_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            PayrollCalculator.CalculatePayroll(MakeEmployee(10000m), 1_000_000m, 3, 2026));
    }

    [Theory]
    [InlineData(0, 2026)]   // month too low
    [InlineData(13, 2026)]  // month too high
    [InlineData(2, 2026)]   // February 2026: before the 2026/27 tax year
    [InlineData(3, 2027)]   // March 2027: after the 2026/27 tax year
    public void InvalidPayPeriod_Throws(int month, int year)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PayrollCalculator.CalculatePayroll(MakeEmployee(20000m), 0m, month, year));
    }

    [Theory]
    [InlineData(3, 2026)]   // first valid month
    [InlineData(12, 2026)]
    [InlineData(1, 2027)]
    [InlineData(2, 2027)]   // last valid month
    public void ValidPayPeriod_DoesNotThrow(int month, int year)
    {
        var exception = Record.Exception(() =>
            PayrollCalculator.CalculatePayroll(MakeEmployee(20000m), 0m, month, year));

        Assert.Null(exception);
    }
}