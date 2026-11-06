using Microsoft.AspNetCore.Mvc;
using Payroll.Api.Controllers;
using Payroll.Api.Requests;
using Xunit;

namespace Payroll.Tests;

public class PayrollControllerTests
{
    private readonly FakeEmployeeRepository _repo = new();
    private readonly FakePayrollRepository _payrollRepo = new();
    private readonly PayrollController _controller;

    public PayrollControllerTests()
    {
        _controller = new PayrollController(_repo, _payrollRepo);
        _repo.Seed(new Employee
        {
            Id = 1,
            FirstName = "Test",
            LastName = "Employee",
            Email = "test@example.com",
            Position = "Developer",
            Salary = 30000m,
            HireDate = new DateTime(2025, 1, 1),
            DateOfBirth = new DateTime(1990, 1, 1)
        });
    }

    [Fact]
    public void Preview_UnknownEmployee_ReturnsNotFound()
    {
        var result = _controller.Preview(999, 3, 2026);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void Preview_ValidRequest_ReturnsOk_WithCalculatedPayslip()
    {
        var result = _controller.Preview(1, 3, 2026);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var record = Assert.IsType<PayrollRecord>(ok.Value);
        Assert.Equal(4681.00m, record.PAYE);
        Assert.Equal(177.12m, record.UIF);
        Assert.Equal(25141.88m, record.NetPay);
    }

    [Fact]
    public void Preview_InvalidMonth_ReturnsBadRequest()
    {
        var result = _controller.Preview(1, 13, 2026);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Preview_DeductionsLargerThanEarnings_ReturnsBadRequest()
    {
        var result = _controller.Preview(1, 3, 2026, otherDeductions: 1_000_000m);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Process_ValidRequest_Returns201_AndSavesTheRecord()
    {
        var request = new ProcessPayrollRequest { EmployeeId = 1, Month = 3, Year = 2026 };

        var result = _controller.Process(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(PayrollController.GetRecords), created.ActionName);

        var record = Assert.IsType<PayrollRecord>(created.Value);
        Assert.True(record.Id > 0);
        Assert.Equal(4681.00m, record.PAYE);
        Assert.Single(_payrollRepo.GetAllPayrollRecords());
    }

    [Fact]
    public void Process_UnknownEmployee_ReturnsNotFound()
    {
        var request = new ProcessPayrollRequest { EmployeeId = 999, Month = 3, Year = 2026 };

        var result = _controller.Process(request);

        Assert.IsType<NotFoundResult>(result.Result);
        Assert.Empty(_payrollRepo.GetAllPayrollRecords());
    }

    [Fact]
    public void Process_SamePeriodTwice_SecondReturnsConflict_AndSavesOnlyOnce()
    {
        var request = new ProcessPayrollRequest { EmployeeId = 1, Month = 3, Year = 2026 };

        _controller.Process(request);
        var second = _controller.Process(request);

        Assert.IsType<ConflictObjectResult>(second.Result);
        Assert.Single(_payrollRepo.GetAllPayrollRecords());
    }

    [Fact]
    public void Process_DifferentMonths_AreBothAllowed()
    {
        _controller.Process(new ProcessPayrollRequest { EmployeeId = 1, Month = 3, Year = 2026 });
        var april = _controller.Process(new ProcessPayrollRequest { EmployeeId = 1, Month = 4, Year = 2026 });

        Assert.IsType<CreatedAtActionResult>(april.Result);
        Assert.Equal(2, _payrollRepo.GetAllPayrollRecords().Count);
    }

    [Fact]
    public void Process_InvalidPeriod_ReturnsBadRequest_AndSavesNothing()
    {
        var request = new ProcessPayrollRequest { EmployeeId = 1, Month = 2, Year = 2026 };   // before the 2026/27 tax year

        var result = _controller.Process(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(_payrollRepo.GetAllPayrollRecords());
    }
}