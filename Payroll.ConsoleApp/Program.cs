using System.Data.Common;
using System.Linq.Expressions;
using System.Net;
using Microsoft.Data.SqlClient;
using System.Globalization;
System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-ZA");

List<Employee> employees = new List<Employee>
{
    new Employee {Id = 1, FirstName = "Losman", LastName = "Bambo", Department = Departments.IT, Salary = 50000m, HireDate = new DateTime(2020, 1, 15), Email = "losmanbambo@company.com", Position = "Software Engineer"},
    new Employee {Id = 2, FirstName = "Thabo", LastName = "Mokoena", Department = Departments.HR, Salary = 60000m, HireDate = new DateTime(2019, 3, 10), Email = "thabomokoena@company.com", Position = "HR Specialist"},
    new Employee {Id = 3, FirstName = "Phuti", LastName = "Moreki", Department = Departments.Finance, Salary = 70000m, HireDate = new DateTime(2018, 5, 20), Email = "phutimoreki@company.com", Position = "Financial Analyst"},
    new Employee {Id = 4, FirstName = "Neo", LastName = "Mabena", Department = Departments.Sales, Salary = 55000m, HireDate = new DateTime(2021, 7, 5), Email = "neomabena@company.com", Position = "Sales Associate"},
    new Employee {Id = 5, FirstName = "Kabelo", LastName = "Mokgosi", Department = Departments.Marketing, Salary = 65000m, HireDate = new DateTime(2020, 9, 12), Email = "kabelomokgosi@company.com", Position = "Marketing Specialist"}
};

static void DisplayEmployees(List<Employee> employees)
{
    Console.WriteLine("--------------------------------------------");
    Console.WriteLine("--------------------------------------------");

    Console.WriteLine(("EMPLOYEE MANAGEMENT SYSTEM"));

    Console.WriteLine("--------------------------------------------");
    Console.WriteLine("--------------------------------------------");

    foreach (var employee in employees)
    {
        Console.WriteLine($"Id: {employee.Id}\nName: {employee.FirstName} {employee.LastName}\nDepartment: {employee.Department}\nPosition: {employee.Position}\nSalary: {employee.Salary}\nHire Date: {employee.HireDate.ToShortDateString()}\nEmail: {employee.Email}\n");
    }

    Console.WriteLine("--------------------------------------------");
}



static void FindEmployeesByDepartment(List<Employee> employees)
{
    Console.WriteLine("Select a department: \n1. IT\n2. HR\n3. Finance\n4. Sales\n5. Marketing");

    Console.Write("Enter department: ");
    string? userInput = Console.ReadLine();

    if (!int.TryParse(userInput, out int departmentChoice) || departmentChoice < 1 || departmentChoice > 5)
    {
        Console.WriteLine("Invalid input. Please enter a valid department number.");
        return;
    }

    Departments selectedDepartment = departmentChoice switch
    {
        1 => Departments.IT,
        2 => Departments.HR,
        3 => Departments.Finance,
        4 => Departments.Sales,
        5 => Departments.Marketing,
        _ => throw new InvalidOperationException("Invalid department choice.")
    };
    var employeesInDepartment = employees.Where(e => e.Department == selectedDepartment);
    DisplayEmployees(employeesInDepartment.ToList());
}


static void FindEmployeesBySalary(List<Employee> employees)
{
    Console.WriteLine("Enter minimum salary: ");
    string? minSalaryInput = Console.ReadLine();

    if (!decimal.TryParse(minSalaryInput, out decimal minSalary))
    {
        Console.WriteLine("Invalid input. Please enter a valid minimum salary.");
        return;
    }

    var employeesInSalaryRange = employees.Where(e => e.Salary >= minSalary).ToList();

    if (!employeesInSalaryRange.Any())
    {
        Console.WriteLine("No employees found with salary greater than or equal to the specified amount.");
        return;
    }

    DisplayEmployees(employeesInSalaryRange.ToList());
}


static void AddEmployee(List<Employee> employees, DatabaseHelper dbHelper)
{
    //Getting new enployee Id
    int newId;
    while (true)
    {
        Console.Write("Enter employee ID: ");
        string? idInput = Console.ReadLine();

        if (!int.TryParse(idInput, out newId))
        {
            Console.WriteLine("Invalid input. Please enter a valid employee ID.");
            continue;
        }

        if (employees.Any(e => e.Id == newId))
        {
            Console.WriteLine("Employee ID already exists. Please enter a unique employee ID.");
            continue;
        }

        break;
    }


    //employee first name
    string? firstName;
    while (true)
    {
        Console.Write("Enter first name: ");
        firstName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(firstName))
        {
            Console.WriteLine("First name cannot be empty. Please enter a valid first name.");
            continue;
        }

        break;
    }


    //employee last name
    string? lastName;
    while (true)
    {
        Console.Write("Enter last name: ");
        lastName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(lastName))
        {
            Console.WriteLine("Last name cannot be empty. Please enter a valid last name.");
            continue;
        }

        break;
    }


    //employee email
    string? email;
    while (true)
    {
        Console.Write("Enter email: ");
        email = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Email cannot be empty. Please enter a valid email.");
            continue;
        }

        break;
    }


    //employee department
    Departments selectedDepartment = GetDepartment();

    //employee position
    string? position;
    while (true)
    {
        Console.Write("Enter position: ");
        position = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(position))
        {
            Console.WriteLine("Position cannot be empty. Please enter a valid position.");
            continue;
        }

        break;
    }


    //employee salary
    decimal salary;
    while (true)
    {
        Console.Write("Enter salary: ");
        string? salaryInput = Console.ReadLine();

        if (!decimal.TryParse(salaryInput, out salary) || salary < 0)
        {
            Console.WriteLine("Invalid input. Please enter a valid salary.");
            continue;
        }

        break;
    }


    //employee hire date
    DateTime hireDate;
    while (true)
    {
        Console.Write("Enter hire date (yyyy-MM-dd): ");
        string? hireDateInput = Console.ReadLine();

        if (!DateTime.TryParseExact(hireDateInput, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out hireDate))
        {
            Console.WriteLine("Invalid input. Please enter a valid hire date in the format yyyy-MM-dd.");
            continue;
        }

        break;
    }


    Employee newEmployee = new Employee
    {
        Id = newId,
        FirstName = firstName,
        LastName = lastName,
        Email = email,
        Department = selectedDepartment,
        Position = position,
        Salary = salary,
        HireDate = hireDate
    };

    try
    {
        bool success = dbHelper.AddEmployee(newEmployee);
        if (success)
        {
            employees.Add(newEmployee);
            Console.WriteLine("Employee added successfully!");
        }
        else
        {
            Console.WriteLine("Failed to add employee to the database.");
        }
    }
    catch (SqlException ex)
    {
        Console.WriteLine($"Database error: {ex.Message}");
    }
}


static void RestoreEmployeeDetails(Employee employee, Employee originalEmployee)
{
    employee.FirstName = originalEmployee.FirstName;
    employee.LastName = originalEmployee.LastName;
    employee.Email = originalEmployee.Email;
    employee.Department = originalEmployee.Department;
    employee.Position = originalEmployee.Position;
    employee.Salary = originalEmployee.Salary;
    employee.HireDate = originalEmployee.HireDate;
    employee.DateOfBirth = originalEmployee.DateOfBirth;
}


static bool HasChanges(Employee employee, Employee originalEmployee)
{
    return employee.FirstName != originalEmployee.FirstName ||
           employee.LastName != originalEmployee.LastName ||
           employee.Email != originalEmployee.Email ||
           employee.Department != originalEmployee.Department ||
           employee.Position != originalEmployee.Position ||
           employee.Salary != originalEmployee.Salary ||
           employee.HireDate != originalEmployee.HireDate ||
           employee.DateOfBirth != originalEmployee.DateOfBirth;
}


static void UpdateEmployee(List<Employee> employees, DatabaseHelper dbHelper)
{
    Console.WriteLine("Enter employee Id to update: ");
    string? employeeId = Console.ReadLine();

    if (!int.TryParse(employeeId, out int id))
    {
        Console.WriteLine("Invalid input. Please enter a valid employee ID.");
        return;
    }

    var employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        Console.WriteLine("Employee not found.");
        return;
    }

    Employee originalEmployee = new Employee
    {
        Id = employee.Id,
        FirstName = employee.FirstName,
        LastName = employee.LastName,
        Email = employee.Email,
        Department = employee.Department,
        Position = employee.Position,
        Salary = employee.Salary,
        HireDate = employee.HireDate,
        DateOfBirth = employee.DateOfBirth
    };


    while (true)
    {
        Console.WriteLine("What would you like to update:");
        Console.WriteLine("1. First Name");
        Console.WriteLine("2. Last Name");
        Console.WriteLine("3. Email");
        Console.WriteLine("4. Department");
        Console.WriteLine("5. Position");
        Console.WriteLine("6. Salary");
        Console.WriteLine("7. Hire Date");
        Console.WriteLine("8. Date of Birth");

        string? choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                Console.Write("Enter new first name: ");
                string? newFirstName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newFirstName))
                {
                    Console.WriteLine("First name cannot be empty. Please enter a valid first name.");
                    continue;
                }
                employee.FirstName = newFirstName;
                Console.WriteLine("First name staged locally for update. Changes will be saved to the database after confirmation.");
                break;
            case "2":
                Console.Write("Enter new last name: ");
                string? newLastName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newLastName))
                {
                    Console.WriteLine("Last name cannot be empty. Please enter a valid last name.");
                    continue;
                }
                employee.LastName = newLastName;
                Console.WriteLine("Last name staged locally for update. Changes will be saved to the database after confirmation.");
                break;
            case "3":
                Console.Write("Enter new email: ");
                string? newEmail = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newEmail))
                {
                    Console.WriteLine("Email cannot be empty. Please enter a valid email.");
                    continue;
                }
                employee.Email = newEmail;
                Console.WriteLine("Email staged locally for update. Changes will be saved to the database after confirmation.");
                break;
            case "4":
                Departments newDepartment = GetDepartment();
                employee.Department = newDepartment;
                Console.WriteLine("Department staged locally for update. Changes will be saved to the database after confirmation.");
                break;
            case "5":
                Console.Write("Enter new position: ");
                string? newPosition = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newPosition))
                {
                    Console.WriteLine("Position cannot be empty. Please enter a valid position.");
                    continue;
                }
                employee.Position = newPosition;
                Console.WriteLine("Position staged locally for update. Changes will be saved to the database after confirmation.");
                break;
            case "6":
                Console.Write("Enter new salary: ");
                if (decimal.TryParse(Console.ReadLine(), out decimal newSalary) && newSalary > 0)
                {
                    employee.Salary = newSalary;
                    Console.WriteLine("Salary staged locally for update. Changes will be saved to the database after confirmation.");
                }
                else
                {
                    Console.WriteLine("Invalid salary input.");
                    continue;
                }
                break;

            case "7":
                Console.Write("Enter new hire date (yyyy-MM-dd): ");
                if (DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime newHireDate))
                {
                    employee.HireDate = newHireDate;
                    Console.WriteLine("Hire date staged locally for update. Changes will be saved to the database after confirmation.");
                }
                else
                {
                    Console.WriteLine("Invalid hire date input.");
                    continue;
                }
                break;

            case "8":
                Console.Write("Enter new date of birth (yyyy-MM-dd): ");
                if (DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime newDob))
                {
                    employee.DateOfBirth = newDob;
                    Console.WriteLine("Date of birth staged locally for update. Changes will be saved to the database after confirmation.");
                }
                else
                {
                    Console.WriteLine("Invalid date format. Please use yyyy-MM-dd.");
                    continue;
                }
                break;
            default:
                Console.WriteLine("Invalid option.");
                break;
        }

        Console.WriteLine("Do you want to update another field for this employee? (y/n):");
        string? updateAnotherField = Console.ReadLine();
        if (updateAnotherField?.Trim().ToLower() != "y")
        {
            break;
        }
    }


    // Check if any fields were actually modified
    if (!HasChanges(employee, originalEmployee))
    {
        Console.WriteLine("No changes were made.");
        return;
    }

    string? confirmation = null;

    while (true)
    {
        Console.WriteLine("Are you sure you want to save the changes to the database? (y/n):");
        confirmation = Console.ReadLine()?.Trim().ToLower();

        if (confirmation == "y" || confirmation == "n")
        {
            break;
        }

        Console.WriteLine("Please enter y or n.");
    }

    //if it's a no
    if (confirmation == "n")
    {
        RestoreEmployeeDetails(employee, originalEmployee);
        Console.WriteLine("Changes reverted. No updates were made to the database.");
        return;
    }

    try
    {
        bool updated = dbHelper.UpdateEmployee(employee);
        if (updated)
        {
            Console.WriteLine("Employee updated successfully!");
        }
        else
        {
            RestoreEmployeeDetails(employee, originalEmployee);
            Console.WriteLine("Failed to update employee. Changes reverted.");
        }
    }
    catch (SqlException ex)
    {
        RestoreEmployeeDetails(employee, originalEmployee);
        Console.WriteLine($"Database error: {ex.Message}. Changes reverted.");
    }
}


static void DeleteEmployee(List<Employee> employees, DatabaseHelper dbHelper)
{
    while (true)
    {
        Console.WriteLine("Enter employee ID to delete: ");
        string? userInput = Console.ReadLine();

        if (!int.TryParse(userInput, out int employeeId))
        {
            Console.WriteLine("Invalid input. Please enter a valid employee ID.");
            continue;
        }

        var employee = employees.FirstOrDefault(e => e.Id == employeeId);
        if (employee == null)
        {
            Console.WriteLine("Employee not found.");
            continue;
        }

        while (true)
        {
            Console.WriteLine("Are you sure you want to delete this employee? (y/n):");
            string? confirmation = Console.ReadLine();

            if (confirmation?.Trim().ToLower() == "n")
            {
                Console.WriteLine("Deletion cancelled.");
                return;
            }
            else if (confirmation?.Trim().ToLower() == "y")
            {
                try
                {
                    bool deleted = dbHelper.DeleteEmployee(employeeId);
                    if (deleted)
                    {
                        employees.Remove(employee);
                        Console.WriteLine("Employee deleted successfully!");
                    }
                    else
                    {
                        Console.WriteLine("Failed to delete employee from the database.");
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Database error: {ex.Message}");
                }

                return;
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
                continue;
            }
        }
    }
}


static Departments GetDepartment()
{
    while (true)
    {
        Console.WriteLine("Select a department: \n1. IT\n2. HR\n3. Finance\n4. Sales\n5. Marketing");
        string? userInput = Console.ReadLine();

        if (!int.TryParse(userInput, out int departmentChoice) || departmentChoice < 1 || departmentChoice > 5)
        {
            Console.WriteLine("Invalid input. Please enter a valid department number.");
            continue;
        }

        return departmentChoice switch
        {
            1 => Departments.IT,
            2 => Departments.HR,
            3 => Departments.Finance,
            4 => Departments.Sales,
            5 => Departments.Marketing,
            _ => Departments.IT // Default to IT department, it won't show the rest thanks to the departmentChoice above
        };
    }
}


static void ShowMenu(List<Employee> employees, DatabaseHelper dbHelper)
{
    while (true)
    {
        Console.WriteLine("Select an option:");
        Console.WriteLine("1. Display all employees");
        Console.WriteLine("2. Find employee by ID");
        Console.WriteLine("3. Find employees by department");
        Console.WriteLine("4. Find employees by salary");
        Console.WriteLine("5. Add Employee");
        Console.WriteLine("6. Update Employee");
        Console.WriteLine("7. Delete Employee");
        Console.WriteLine("8. Process Payroll");
        Console.WriteLine("9. View Payroll History");
        Console.WriteLine("10. Run Payroll Test");
        Console.WriteLine("11. Exit");

        string? userInput = Console.ReadLine();

        switch (userInput)
        {
            case "1":
                DisplayEmployees(employees);
                break;
            case "2":
                Console.WriteLine("Enter employee ID: ");
                string? idInput = Console.ReadLine();
                if (!int.TryParse(idInput, out int employeeId))
                {
                    Console.WriteLine("Invalid input. Please enter a valid employee ID.");
                    break;
                }
                Employee? employee = dbHelper.GetEmployeeById(employeeId);
                if (employee != null)
                {
                    Console.WriteLine($"Found: {employee.FirstName} {employee.LastName}");
                }
                else
                {
                    Console.WriteLine("Employee not found.");
                }
                break;
            case "3":
                FindEmployeesByDepartment(employees);
                break;
            case "4":
                FindEmployeesBySalary(employees);
                break;
            case "5":
                AddEmployee(employees, dbHelper);
                break;
            case "6":
                UpdateEmployee(employees, dbHelper);
                break;
            case "7":
                DeleteEmployee(employees, dbHelper);
                break;
            case "8":
                ProcessPayrollMenu(dbHelper);
                break;
            case "9":
                DisplayPayrollHistory(dbHelper);
                break;
            case "10":
                RunPayrollTest(dbHelper);
                break;
            case "11":
                Console.WriteLine("Exiting the program. Goodbye!");
                return;
            default:
                Console.WriteLine("Invalid option. Please try again.");
                break;
        }

        Console.ReadKey();
    }
}


static void RunPayrollTest(DatabaseHelper dbHelper)
{
    Console.WriteLine("Enter Employee ID to test payroll:");
    if (!int.TryParse(Console.ReadLine(), out int empId))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    Employee? employee = dbHelper.GetEmployeeById(empId);
    if (employee == null)
    {
        Console.WriteLine("Employee not found.");
        return;
    }

    try
    {
        // Testing October 2026 payroll as requested
        int payMonth = 10;
        int payYear = 2026;

        decimal otherDeductions = 0m;

        while (true)
        {
            Console.WriteLine("Enter other deductions amount (e.g., 0 for none, or 10001 to test rejection):");
            string? input = Console.ReadLine();

            if (decimal.TryParse(input, out otherDeductions) && otherDeductions >= 0m)
            {
                break; // Valid input, exit the loop
            }

            Console.WriteLine("Invalid input. Please enter a valid non-negative decimal amount.");
        }

        PayrollRecord record = PayrollCalculator.CalculatePayroll(employee, otherDeductions, payMonth, payYear);

        Console.WriteLine("\n--- Payroll Results ---");
        Console.WriteLine($"Employee: {employee.FirstName} {employee.LastName}");
        Console.WriteLine($"Date of Birth: {employee.DateOfBirth?.ToString("yyyy-MM-dd")}");
        Console.WriteLine($"Tax Year End Used: {Employee.GetTaxYearEnd(payMonth, payYear):yyyy-MM-dd}");
        Console.WriteLine($"Age as of Tax Year End: {employee.GetAge(Employee.GetTaxYearEnd(payMonth, payYear))}");
        Console.WriteLine($"Gross Pay: R{record.GrossPay:N2}");
        Console.WriteLine($"Other Deductions: R{record.OtherDeductions:N2}");
        Console.WriteLine($"PAYE Deducted: R{record.PAYE:N2}");
        Console.WriteLine($"UIF Deducted: R{record.UIF:N2}");
        Console.WriteLine($"Net Pay: R{record.NetPay:N2}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Payroll Error: {ex.Message}");
    }
}


string connectionString = @"Server=DESKTOP-GT5UN44\SQLEXPRESS;Database=EmployeeManagementDB;Integrated Security=True;TrustServerCertificate=True;";

DatabaseHelper dbHelper = new DatabaseHelper(connectionString);

if (dbHelper.TestConnection())
{
    Console.WriteLine("Connection successful! Connected to EmployeeManagementDB.");
}
else
{
    Console.WriteLine("Connection failed. Please check your SQL Server settings.");
}


List<Employee> databaseEmployees = dbHelper.GetAllEmployees();

Console.WriteLine($"Employees loaded from database: {databaseEmployees.Count}");


foreach (Employee employee in databaseEmployees)
{
    Console.WriteLine($"Loaded ID: {employee.Id}");
}



static void ProcessPayrollMenu(DatabaseHelper dbHelper)
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("--- Process Payroll ---");
        Console.WriteLine("(Type '0' for Employee ID or leave blank to return to Main Menu)\n");

        Console.Write("Enter Employee ID: ");
        string? inputId = Console.ReadLine();

        // Allow user to exit back to main menu easily if they change their mind
        if (string.IsNullOrWhiteSpace(inputId) || inputId == "0")
        {
            return;
        }

        if (!int.TryParse(inputId, out int employeeId))
        {
            Console.WriteLine("\nInvalid ID format. Press any key to try again...");
            Console.ReadKey();
            continue; // Go back to the start of the loop
        }

        Employee? employee = dbHelper.GetEmployeeById(employeeId);
        if (employee == null)
        {
            Console.WriteLine("\nEmployee not found in the database. Press any key to try again...");
            Console.ReadKey();
            continue;
        }

        Console.Write("Enter Pay Month (1-12): ");
        if (!int.TryParse(Console.ReadLine(), out int payMonth) || payMonth < 1 || payMonth > 12)
        {
            Console.WriteLine("\nInvalid month. Must be between 1 and 12. Press any key to try again...");
            Console.ReadKey();
            continue;
        }

        Console.Write("Enter Pay Year (e.g., 2026): ");
        if (!int.TryParse(Console.ReadLine(), out int payYear) || payYear < 2026)
        {
            Console.WriteLine("\nInvalid year. Must be 2026 or later. Press any key to try again...");
            Console.ReadKey();
            continue;
        }

        // Check for duplicate payroll before moving forward (now safe because month/year are valid)
        if (dbHelper.PayrollExists(employeeId, payMonth, payYear))
        {
            Console.WriteLine($"\nWarning: Payroll for {employee.FirstName} {employee.LastName} for month {payMonth}/{payYear} has already been processed!");
            Console.WriteLine("Press any key to try a different period or employee...");
            Console.ReadKey();
            continue;
        }

        Console.Write("Enter Other Deductions (e.g., 1500.00): ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal otherDeductions))
        {
            Console.WriteLine("\nInvalid deduction amount. Press any key to try again...");
            Console.ReadKey();
            continue;
        }

        try
        {
            // 1. Calculate payroll (will catch out-of-range deductions here too)
            PayrollRecord record = PayrollCalculator.CalculatePayroll(employee, otherDeductions, payMonth, payYear);

            // 2. Save to database using dbHelper
            bool isSaved = dbHelper.SavePayrollRecord(record);

            if (isSaved)
            {
                Console.WriteLine($"\nPayroll successfully processed and saved for {employee.FirstName} {employee.LastName}!");
                Console.WriteLine($"Gross Pay: {record.GrossPay:C} | Net Pay: {record.NetPay:C}");

                Console.WriteLine("\nPress any key to return to the Main Menu...");
                Console.ReadKey();
                return; // Success! Now we can exit back to the main menu.
            }
            else
            {
                Console.WriteLine("\nFailed to save payroll record to the database.");
                Console.WriteLine("Press any key to try again...");
                Console.ReadKey();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nValidation/Error: {ex.Message}");
            Console.WriteLine("Press any key to try again...");
            Console.ReadKey();
        }
    }
}


static void DisplayPayrollHistory(DatabaseHelper dbHelper)
{
    Console.Clear();
    Console.WriteLine("--------------------------------------------");
    Console.WriteLine("              PAYROLL HISTORY               ");
    Console.WriteLine("--------------------------------------------");

    List<PayrollRecord> records = dbHelper.GetAllPayrollRecords();

    if (records.Count == 0)
    {
        Console.WriteLine("No payroll records found in the database.");
        return;
    }

    foreach (var record in records)
    {
        Console.WriteLine($"Record ID: {record.Id}");
        Console.WriteLine($"Employee ID: {record.EmployeeId}");
        Console.WriteLine($"Period: {record.PayMonth}/{record.PayYear}");
        Console.WriteLine($"Gross Pay: {record.GrossPay:C}");
        Console.WriteLine($"Other Deductions: {record.OtherDeductions:C}");
        Console.WriteLine($"PAYE: {record.PAYE:C}");
        Console.WriteLine($"UIF: {record.UIF:C}");
        Console.WriteLine($"Net Pay: {record.NetPay:C}");
        Console.WriteLine($"Processed At: {record.ProcessedAt:yyyy-MM-dd HH:mm}");
        Console.WriteLine("--------------------------------------------");
    }
    Console.WriteLine("\nPress any key to return to the Main Menu...");
}



ShowMenu(databaseEmployees, dbHelper);