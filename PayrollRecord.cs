public class PayrollRecord
{
    public int Id {get; set;}
    public int EmployeeId {get; set;}
    public int PayMonth {get; set;}
    public int PayYear {get; set;}
    public decimal GrossPay {get; set;}
    public decimal OtherDeductions {get; set;}
    public decimal PAYE {get; set;}
    public decimal UIF {get; set;}
    public decimal NetPay {get; set;}
    public DateTime ProcessedAt {get; set;}
}