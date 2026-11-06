namespace Payroll.Tests;

public class FakePayrollRepository : IPayrollRepository
{
    private readonly List<PayrollRecord> _records = new();
    private int _nextId = 1;

    public int SavePayrollRecord(PayrollRecord record)
    {
        record.Id = _nextId++;
        _records.Add(record);
        return record.Id;
    }

    public bool PayrollExists(int employeeId, int payMonth, int payYear) =>
        _records.Any(r => r.EmployeeId == employeeId && r.PayMonth == payMonth && r.PayYear == payYear);

    public List<PayrollRecord> GetAllPayrollRecords() => _records.ToList();
}