using Microsoft.AspNetCore.Mvc;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayrollController : ControllerBase
{
    private readonly DatabaseHelper _db;

    public PayrollController(DatabaseHelper db)
    {
        _db = db;
    }

    [HttpGet("{employeeId}")]
    public ActionResult<PayrollRecord> Preview(int employeeId, int month, int year, decimal otherDeductions = 0m)
    {
        var employee = _db.GetEmployeeById(employeeId);

        if (employee == null)
        {
            return NotFound();
        }

        try
        {
            var record = PayrollCalculator.CalculatePayroll(employee, otherDeductions, month, year);
            return Ok(record);
        }
        catch (Exception ex) when (ex is ArgumentOutOfRangeException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }
    }
}