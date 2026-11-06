public interface IPayrollRepository
{
    int SavePayrollRecord(PayrollRecord record);
    bool PayrollExists(int employeeId, int payMonth, int payYear);
    List<PayrollRecord> GetAllPayrollRecords();
}