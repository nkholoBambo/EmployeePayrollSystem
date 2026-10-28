using System;
using Microsoft.Data.SqlClient;
using System.Data;

public class DatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public bool TestConnection()
    {
        using var connection = new SqlConnection(_connectionString);
        try
        {
            connection.Open();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            return false;
        }
    }

    public List<Employee> GetAllEmployees()
    {
        List<Employee> employees = new List<Employee>();

        try
        {
            using var connection = new SqlConnection(_connectionString);

            connection.Open();

            // 1. Add DateOfBirth to your SELECT query
            using var command = new SqlCommand("SELECT Id, FirstName, LastName, Email, Department, Position, Salary, HireDate, DateOfBirth FROM Employees", connection);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                int id = reader.GetInt32(reader.GetOrdinal("Id"));
                string firstName = reader.GetString(reader.GetOrdinal("FirstName"));
                string lastName = reader.GetString(reader.GetOrdinal("LastName"));
                string email = reader.GetString(reader.GetOrdinal("Email"));
                Departments department = Enum.Parse<Departments>(reader.GetString(reader.GetOrdinal("Department")));
                string position = reader.GetString(reader.GetOrdinal("Position"));
                decimal salary = reader.GetDecimal(reader.GetOrdinal("Salary"));
                DateTime hireDate = reader.GetDateTime(reader.GetOrdinal("HireDate"));

                // 2. Safely check for DBNull for DateOfBirth
                int dobOrdinal = reader.GetOrdinal("DateOfBirth");
                DateTime? dateOfBirth = reader.IsDBNull(dobOrdinal) ? (DateTime?)null : reader.GetDateTime(dobOrdinal);

                var employee = new Employee
                {
                    Id = id,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Department = department,
                    Position = position,
                    Salary = salary,
                    HireDate = hireDate,
                    DateOfBirth = dateOfBirth
                };

                employees.Add(employee);
            }
        }
        catch (SqlException)
        {
            Console.WriteLine("A database error occurred while retrieving employees.");
            throw;
        }

        return employees;
    }
    public Employee? GetEmployeeById(int id)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // 1. Add DateOfBirth to your SELECT query
            using var command = new SqlCommand("SELECT Id, FirstName, LastName, Email, Department, Position, Salary, HireDate, DateOfBirth FROM Employees WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                int employeeId = reader.GetInt32(reader.GetOrdinal("Id"));
                string firstName = reader.GetString(reader.GetOrdinal("FirstName"));
                string lastName = reader.GetString(reader.GetOrdinal("LastName"));
                string email = reader.GetString(reader.GetOrdinal("Email"));
                Departments department = Enum.Parse<Departments>(reader.GetString(reader.GetOrdinal("Department")));
                string position = reader.GetString(reader.GetOrdinal("Position"));
                decimal salary = reader.GetDecimal(reader.GetOrdinal("Salary"));
                DateTime hireDate = reader.GetDateTime(reader.GetOrdinal("HireDate"));

                // 2. Safely read DateOfBirth checking for DBNull
                int dobOrdinal = reader.GetOrdinal("DateOfBirth");
                DateTime? dateOfBirth = reader.IsDBNull(dobOrdinal) ? (DateTime?)null : reader.GetDateTime(dobOrdinal);

                return new Employee
                {
                    Id = employeeId,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Department = department,
                    Position = position,
                    Salary = salary,
                    HireDate = hireDate,
                    DateOfBirth = dateOfBirth
                };
            }
        }
        catch (SqlException)
        {
            Console.WriteLine("A database error occurred while retrieving the employee.");
            throw;
        }

        return null;
    }

    public bool AddEmployee(Employee employee)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = new SqlCommand(
            "INSERT INTO Employees (Id, FirstName, LastName, Email, Department, Position, Salary, HireDate, DateOfBirth) " +
            "VALUES (@Id, @FirstName, @LastName, @Email, @Department, @Position, @Salary, @HireDate, @DateOfBirth)",
            connection);

        command.Parameters.AddWithValue("@Id", employee.Id);
        command.Parameters.AddWithValue("@FirstName", employee.FirstName);
        command.Parameters.AddWithValue("@LastName", employee.LastName);
        command.Parameters.AddWithValue("@Email", employee.Email);
        command.Parameters.AddWithValue("@Department", employee.Department.ToString());
        command.Parameters.AddWithValue("@Position", employee.Position);
        command.Parameters.AddWithValue("@Salary", employee.Salary);
        command.Parameters.AddWithValue("@HireDate", employee.HireDate);
        command.Parameters.AddWithValue("@DateOfBirth", employee.DateOfBirth ?? (object)DBNull.Value);

        int rowsAffected = command.ExecuteNonQuery();
        return rowsAffected > 0;
    }

    public bool UpdateEmployee(Employee employee)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = new SqlCommand(
            "UPDATE Employees SET FirstName = @FirstName, LastName = @LastName, Email = @Email, Department = @Department, Position = @Position, Salary = @Salary, HireDate = @HireDate, DateOfBirth = @DateOfBirth WHERE Id = @Id",
            connection);

        command.Parameters.AddWithValue("@Id", employee.Id);
        command.Parameters.AddWithValue("@FirstName", employee.FirstName);
        command.Parameters.AddWithValue("@LastName", employee.LastName);
        command.Parameters.AddWithValue("@Email", employee.Email);
        command.Parameters.AddWithValue("@Department", employee.Department.ToString());
        command.Parameters.AddWithValue("@Position", employee.Position);
        command.Parameters.AddWithValue("@Salary", employee.Salary);
        command.Parameters.AddWithValue("@HireDate", employee.HireDate);
        command.Parameters.AddWithValue("@DateOfBirth", (object?)employee.DateOfBirth ?? DBNull.Value);

        int rowsAffected = command.ExecuteNonQuery();
        return rowsAffected == 1;
    }

    public bool DeleteEmployee(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        string query = "DELETE FROM Employees WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        int rowsAffected = command.ExecuteNonQuery();
        return rowsAffected == 1;
    }


    public bool SavePayrollRecord(PayrollRecord record)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            string query = @"INSERT INTO PayrollRecords 
                     (EmployeeId, PayMonth, PayYear, GrossPay, OtherDeductions, PAYE, UIF, NetPay, ProcessedAt) 
                     VALUES 
                     (@EmployeeId, @PayMonth, @PayYear, @GrossPay, @OtherDeductions, @PAYE, @UIF, @NetPay, @ProcessedAt)";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // 1. Explicit integer parameters
                command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = record.EmployeeId;
                command.Parameters.Add("@PayMonth", SqlDbType.Int).Value = record.PayMonth;
                command.Parameters.Add("@PayYear", SqlDbType.Int).Value = record.PayYear;

                // 2. Explicit decimal/money parameters with precision and scale matching DECIMAL(12, 2)
                command.Parameters.Add(new SqlParameter("@GrossPay", SqlDbType.Decimal) { Precision = 12, Scale = 2, Value = record.GrossPay });
                command.Parameters.Add(new SqlParameter("@OtherDeductions", SqlDbType.Decimal) { Precision = 12, Scale = 2, Value = record.OtherDeductions });
                command.Parameters.Add(new SqlParameter("@PAYE", SqlDbType.Decimal) { Precision = 12, Scale = 2, Value = record.PAYE });
                command.Parameters.Add(new SqlParameter("@UIF", SqlDbType.Decimal) { Precision = 12, Scale = 2, Value = record.UIF });
                command.Parameters.Add(new SqlParameter("@NetPay", SqlDbType.Decimal) { Precision = 12, Scale = 2, Value = record.NetPay });

                // 3. Explicit datetime parameter
                command.Parameters.Add("@ProcessedAt", SqlDbType.DateTime2).Value = record.ProcessedAt;

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }

    public bool PayrollExists(int employeeId, int payMonth, int payYear)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            string query = "SELECT COUNT(1) FROM PayrollRecords WHERE EmployeeId = @EmployeeId AND PayMonth = @PayMonth AND PayYear = @PayYear";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                cmd.Parameters.AddWithValue("@PayMonth", payMonth);
                cmd.Parameters.AddWithValue("@PayYear", payYear);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
    }


    public List<PayrollRecord> GetAllPayrollRecords()
    {
        // 1. Create an empty list to hold the records
        List<PayrollRecord> recordsList = new List<PayrollRecord>();

        string query = "SELECT * FROM PayrollRecords";

        // 2. Pass your connection string into SqlConnection
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                connection.Open();

                // 3. Capture the reader returned by ExecuteReader()
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    // 4. Loop through each row
                    while (reader.Read())
                    {
                        // 5. Instantiate a new PayrollRecord and map the columns here:
                        PayrollRecord record = new PayrollRecord();
                        record.Id = reader.GetInt32(reader.GetOrdinal("Id"));
                        record.EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId"));
                        record.PayMonth = reader.GetInt32(reader.GetOrdinal("PayMonth"));
                        record.PayYear = reader.GetInt32(reader.GetOrdinal("PayYear"));
                        record.GrossPay = reader.GetDecimal(reader.GetOrdinal("GrossPay"));
                        record.OtherDeductions = reader.GetDecimal(reader.GetOrdinal("OtherDeductions"));
                        record.PAYE = reader.GetDecimal(reader.GetOrdinal("PAYE"));
                        record.UIF = reader.GetDecimal(reader.GetOrdinal("UIF"));
                        record.NetPay = reader.GetDecimal(reader.GetOrdinal("NetPay"));
                        record.ProcessedAt = reader.GetDateTime(reader.GetOrdinal("ProcessedAt"));
                        // 6. Add the populated record to your list
                        recordsList.Add(record);
                    }
                }
            }
        }

        // 7. Return the completed list
        return recordsList;
    }
}