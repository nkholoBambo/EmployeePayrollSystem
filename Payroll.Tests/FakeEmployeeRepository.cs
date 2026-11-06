namespace Payroll.Tests;

public class FakeEmployeeRepository : IEmployeeRepository
{
    private readonly Dictionary<int, Employee> _employees = new();

    public bool FailOnAdd { get; set; }
    public bool FailOnUpdate { get; set; }
    public bool FailOnDelete { get; set; }

    public List<Employee> GetAllEmployees() => _employees.Values.ToList();

    public Employee? GetEmployeeById(int id) =>
        _employees.TryGetValue(id, out var employee) ? employee : null;

    private int _nextId = 1;

    public int AddEmployee(Employee employee)
    {
        var id = _nextId++;
        employee.Id = id;
        _employees[id] = employee;
        return id;
    }

    
    public void Seed(Employee employee) => _employees[employee.Id] = employee;

    public bool UpdateEmployee(Employee employee)
    {
        if (FailOnUpdate || !_employees.ContainsKey(employee.Id)) return false;
        _employees[employee.Id] = employee;
        return true;
    }

    public bool DeleteEmployee(int id)
    {
        if (FailOnDelete) return false;
        return _employees.Remove(id);
    }
}