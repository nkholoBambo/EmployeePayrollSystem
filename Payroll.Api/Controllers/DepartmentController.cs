using Microsoft.AspNetCore.Mvc;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    [HttpGet]
    public ActionResult<string[]> GetAll() => Ok(Enum.GetNames<Departments>());
}