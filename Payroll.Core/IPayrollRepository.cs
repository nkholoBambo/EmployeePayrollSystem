public interface IPayrollRepository
{
    bool SavePayrollRecord(PayrollRecord record);
    bool PayrollExists(int employeeId, int payMonth, int payYear);
    List<PayrollRecord> GetAllPayrollRecords();
}