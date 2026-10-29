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

    [HttpPost]
    public ActionResult<Employee> Create(Employee employee)
    {
        if (_db.GetEmployeeById(employee.Id) != null)
        {
            return Conflict($"An employee with Id {employee.Id} already exists.");
        }

        if (!_db.AddEmployee(employee))
        {
            return StatusCode(500, "The employee could not be saved.");
        }

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }


    [HttpPut("{id}")]
    public IActionResult Update(int id, Employee employee)
    {
        if (id != employee.Id)
        {
            return BadRequest("The Id in the address must match the Id in the body.");
        }

        if (_db.GetEmployeeById(id) == null)
        {
            return NotFound();
        }

        if (!_db.UpdateEmployee(employee))
        {
            return StatusCode(500, "The employee could not be updated.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (_db.GetEmployeeById(id) == null)
        {
            return NotFound();
        }

        if (!_db.DeleteEmployee(id))
        {
            return StatusCode(500, "The employee could not be deleted.");
        }

        return NoContent();
    }
}