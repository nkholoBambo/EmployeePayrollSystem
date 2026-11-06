using Microsoft.AspNetCore.Mvc;
using Payroll.Api.Controllers;
using Xunit;

namespace Payroll.Tests;

public class EmployeesControllerTests
{
    // xUnit builds a fresh instance of this class for every test,
    // so each test starts with an empty repository.
    private readonly FakeEmployeeRepository _repo = new();
    private readonly EmployeesController _controller;

    public EmployeesControllerTests()
    {
        _controller = new EmployeesController(_repo);
    }

    private static Employee MakeEmployee(int id) => new Employee
    {
        Id = id,
        FirstName = "Test",
        LastName = "Employee",
        Email = $"test{id}@example.com",
        Position = "Developer",
        Salary = 30000m,
        HireDate = new DateTime(2025, 1, 1),
        DateOfBirth = new DateTime(1990, 1, 1)
    };

    // ---------- GET ----------

    [Fact]
    public void GetAll_ReturnsOk_WithAllEmployees()
    {
        _repo.Seed(MakeEmployee(1));
        _repo.Seed(MakeEmployee(2));

        var result = _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var employees = Assert.IsType<List<Employee>>(ok.Value);
        Assert.Equal(2, employees.Count);
    }

    [Fact]
    public void GetById_Existing_ReturnsOk()
    {
        var employee = MakeEmployee(1);
        _repo.Seed(employee);

        var result = _controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(employee, ok.Value);
    }

    [Fact]
    public void GetById_Missing_ReturnsNotFound()
    {
        var result = _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ---------- POST ----------

    [Fact]
    public void Create_NewEmployee_Returns201_WithTheDatabaseGeneratedId()
    {
        var result = _controller.Create(MakeEmployee(0));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(EmployeesController.GetById), created.ActionName);

        var saved = Assert.IsType<Employee>(created.Value);
        Assert.True(saved.Id > 0);
        Assert.Equal(saved.Id, created.RouteValues!["id"]);
        Assert.NotNull(_repo.GetEmployeeById(saved.Id));
    }

    

    [Fact]
public void Create_IgnoresAnIdSentByTheCaller()
{
    var result = _controller.Create(MakeEmployee(999));

    var created = Assert.IsType<CreatedAtActionResult>(result.Result);
    var saved = Assert.IsType<Employee>(created.Value);
    Assert.NotEqual(999, saved.Id);
    Assert.Null(_repo.GetEmployeeById(999));
}

    // ---------- PUT ----------

    [Fact]
    public void Update_IdInUrlDiffersFromBody_ReturnsBadRequest()
    {
        var result = _controller.Update(5, MakeEmployee(6));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Update_MissingEmployee_ReturnsNotFound()
    {
        var result = _controller.Update(5, MakeEmployee(5));

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Update_ExistingEmployee_ReturnsNoContent_AndChangesData()
    {
        _repo.Seed(MakeEmployee(5));
        var changed = MakeEmployee(5);
        changed.Salary = 45000m;

        var result = _controller.Update(5, changed);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(45000m, _repo.GetEmployeeById(5)!.Salary);
    }

    [Fact]
    public void Update_WhenSaveFails_Returns500()
    {
        _repo.Seed(MakeEmployee(5));
        _repo.FailOnUpdate = true;

        var result = _controller.Update(5, MakeEmployee(5));

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    // ---------- DELETE ----------

    [Fact]
    public void Delete_MissingEmployee_ReturnsNotFound()
    {
        var result = _controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Delete_ExistingEmployee_ReturnsNoContent_AndRemovesIt()
    {
        _repo.Seed(MakeEmployee(5));

        var result = _controller.Delete(5);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(_repo.GetEmployeeById(5));
    }

    [Fact]
    public void Delete_WhenDeleteFails_Returns500()
    {
        _repo.Seed(MakeEmployee(5));
        _repo.FailOnDelete = true;

        var result = _controller.Delete(5);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }
}