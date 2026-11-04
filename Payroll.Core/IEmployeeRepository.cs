public interface IEmployeeRepository
{
    List<Employee> GetAllEmployees();
    Employee? GetEmployeeById(int id);
    bool AddEmployee(Employee employee);
    bool UpdateEmployee(Employee employee);
    bool DeleteEmployee(int id);
}