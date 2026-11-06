using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Payroll.Api.Requests;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayrollController : ControllerBase
{
    private readonly IEmployeeRepository _employees;
    private readonly IPayrollRepository _payroll;

    public PayrollController(IEmployeeRepository employees, IPayrollRepository payroll)
    {
        _employees = employees;
        _payroll = payroll;
    }

    [HttpGet("{employeeId:int}")]
    public ActionResult<PayrollRecord> Preview(int employeeId, int month, int year, decimal otherDeductions = 0m)
    {
        var employee = _employees.GetEmployeeById(employeeId);

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

    [HttpGet("records")]
    public ActionResult<List<PayrollRecord>> GetRecords()
    {
        return Ok(_payroll.GetAllPayrollRecords());
    }

    [HttpPost]
    public ActionResult<PayrollRecord> Process(ProcessPayrollRequest request)
    {
        var employee = _employees.GetEmployeeById(request.EmployeeId);

        if (employee == null)
        {
            return NotFound();
        }

        PayrollRecord record;

        try
        {
            record = PayrollCalculator.CalculatePayroll(
                employee, request.OtherDeductions, request.Month, request.Year);
        }
        catch (Exception ex) when (ex is ArgumentOutOfRangeException or InvalidOperationException)
        {
            return BadRequest(ex.Message);
        }

        if (_payroll.PayrollExists(request.EmployeeId, request.Month, request.Year))
        {
            return Conflict("Payroll for this employee and period has already been processed.");
        }

        try
        {
            record.Id = _payroll.SavePayrollRecord(record);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            // Two requests slipped past the check at the same moment;
            // the database's unique constraint stopped the second one.
            return Conflict("Payroll for this employee and period has already been processed.");
        }

        return CreatedAtAction(nameof(GetRecords), record);
    }
}