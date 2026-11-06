namespace Payroll.Web.Models;

public class ApiValidationProblem
{
    public Dictionary<string, string[]>? Errors { get; set; }
}