using Microsoft.AspNetCore.Mvc;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly DatabaseHelper _db;

    public EmployeesController(DatabaseHelper db)
    {
        _db = db;
    }

    [HttpGet]
    public ActionResult<List<Employee>> GetAll()
    {
        return Ok(_db.GetAllEmployees());
    }

    [HttpGet("{id}")]
    public ActionResult<Employee> GetById(int id)
    {
        var employee = _db.GetEmployeeById(id);

        if (employee == null)
        {
            return NotFound();
        }

        return Ok(employee);
    }
}