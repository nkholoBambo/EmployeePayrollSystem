using Microsoft.AspNetCore.Mvc;
using Payroll.Api.Controllers;
using Xunit;

namespace Payroll.Tests;

public class PayrollControllerTests
{
    private readonly FakeEmployeeRepository _repo = new();
    private readonly PayrollController _controller;

    public PayrollControllerTests()
    {
        _controller = new PayrollController(_repo);
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
}