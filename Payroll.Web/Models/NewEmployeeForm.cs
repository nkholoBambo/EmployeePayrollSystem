using System.ComponentModel.DataAnnotations;

namespace Payroll.Web.Models;

public class NewEmployeeForm
{
    [Required, Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a department.")]
    public string Department { get; set; } = string.Empty;

    [Required]
    public string Position { get; set; } = string.Empty;

    [Range(0.01, 10000000, ErrorMessage = "Salary must be greater than zero.")]
    public decimal Salary { get; set; }

    [Display(Name = "Hire date")]
    public DateTime HireDate { get; set; }

    [Required(ErrorMessage = "Date of birth is required for payroll.")]
    [Display(Name = "Date of birth")]
    public DateTime? DateOfBirth { get; set; }
}