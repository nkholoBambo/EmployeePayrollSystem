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
        _repo.AddEmployee(MakeEmployee(1));
        _repo.AddEmployee(MakeEmployee(2));

        var result = _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var employees = Assert.IsType<List<Employee>>(ok.Value);
        Assert.Equal(2, employees.Count);
    }

    [Fact]
    public void GetById_Existing_ReturnsOk()
    {
        var employee = MakeEmployee(1);
        _repo.AddEmployee(employee);

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
    public void Create_NewEmployee_Returns201_PointingAtGetById()
    {
        var result = _controller.Create(MakeEmployee(10));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(EmployeesController.GetById), created.ActionName);
        Assert.Equal(10, created.RouteValues!["id"]);
        Assert.NotNull(_repo.GetEmployeeById(10));   // it was really saved
    }

    [Fact]
    public void Create_DuplicateId_ReturnsConflict()
    {
        _repo.AddEmployee(MakeEmployee(10));

        var result = _controller.Create(MakeEmployee(10));

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public void Create_WhenSaveFails_Returns500()
    {
        _repo.FailOnAdd = true;

        var result = _controller.Create(MakeEmployee(10));

        var error = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, error.StatusCode);
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
        _repo.AddEmployee(MakeEmployee(5));
        var changed = MakeEmployee(5);
        changed.Salary = 45000m;

        var result = _controller.Update(5, changed);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(45000m, _repo.GetEmployeeById(5)!.Salary);
    }

    [Fact]
    public void Update_WhenSaveFails_Returns500()
    {
        _repo.AddEmployee(MakeEmployee(5));
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
        _repo.AddEmployee(MakeEmployee(5));

        var result = _controller.Delete(5);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(_repo.GetEmployeeById(5));
    }

    [Fact]
    public void Delete_WhenDeleteFails_Returns500()
    {
        _repo.AddEmployee(MakeEmployee(5));
        _repo.FailOnDelete = true;

        var result = _controller.Delete(5);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }
}