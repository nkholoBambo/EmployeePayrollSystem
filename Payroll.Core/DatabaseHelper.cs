using System;
using Microsoft.Data.SqlClient;
using System.Data;

public class DatabaseHelper : IEmployeeRepository, IPayrollRepository
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

    public int AddEmployee(Employee employee)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = new SqlCommand(
            "INSERT INTO Employees (FirstName, LastName, Email, Department, Position, Salary, HireDate, DateOfBirth) " +
            "OUTPUT INSERTED.Id " +
            "VALUES (@FirstName, @LastName, @Email, @Department, @Position, @Salary, @HireDate, @DateOfBirth)",
            connection);

        command.Parameters.AddWithValue("@FirstName", employee.FirstName);
        command.Parameters.AddWithValue("@LastName", employee.LastName);
        command.Parameters.AddWithValue("@Email", employee.Email);
        command.Parameters.AddWithValue("@Department", employee.Department.ToString());
        command.Parameters.AddWithValue("@Position", employee.Position);
        command.Parameters.Add(new SqlParameter("@Salary", SqlDbType.Decimal)
        {
            Precision = 12,
            Scale = 2,
            Value = employee.Salary
        });
        command.Parameters.AddWithValue("@HireDate", employee.HireDate);
        command.Parameters.AddWithValue("@DateOfBirth", (object?)employee.DateOfBirth ?? DBNull.Value);

        return Convert.ToInt32(command.ExecuteScalar());
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
        command.Parameters.Add(new SqlParameter("@Salary", SqlDbType.Decimal)
        {
            Precision = 12,
            Scale = 2,
            Value = employee.Salary
        });
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

    public int SavePayrollRecord(PayrollRecord record)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string query = @"INSERT INTO PayrollRecords
        (EmployeeId, PayMonth, PayYear, GrossPay, OtherDeductions, PAYE, UIF, NetPay, ProcessedAt)
        OUTPUT INSERTED.Id
        VALUES
        (@EmployeeId, @PayMonth, @PayYear, @GrossPay, @OtherDeductions, @PAYE, @UIF, @NetPay, @ProcessedAt)";

        using var command = new SqlCommand(query, connection);

        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = record.EmployeeId;
        command.Parameters.Add("@PayMonth", SqlDbType.Int).Value = record.PayMonth;
        command.Parameters.Add("@PayYear", SqlDbType.Int).Value = record.PayYear;

        command.Parameters.Add(new SqlParameter("@GrossPay", SqlDbType.Decimal) { Precision = 12, Scale = 2, Value = record.GrossPay });
        command.Parameters.Add(new SqlParameter("@OtherDeductions", SqlDbType.Decimal) { Precision = 12, Scale = 2, Value = record.OtherDeductions });
        command.Parameters.Add(new SqlParameter("@PAYE", SqlDbType.Decimal) { Precision = 12, Scale = 2, Value = record.PAYE });
        command.Parameters.Add(new SqlParameter("@UIF", SqlDbType.Decimal) { Precision = 12, Scale = 2, Value = record.UIF });
        command.Parameters.Add(new SqlParameter("@NetPay", SqlDbType.Decimal) { Precision = 12, Scale = 2, Value = record.NetPay });

        command.Parameters.Add("@ProcessedAt", SqlDbType.DateTime2).Value = record.ProcessedAt;

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool PayrollExists(int employeeId, int payMonth, int payYear)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = new SqlCommand(
            "SELECT COUNT(1) FROM PayrollRecords WHERE EmployeeId = @EmployeeId AND PayMonth = @PayMonth AND PayYear = @PayYear",
            connection);

        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employeeId;
        command.Parameters.Add("@PayMonth", SqlDbType.Int).Value = payMonth;
        command.Parameters.Add("@PayYear", SqlDbType.Int).Value = payYear;

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public List<PayrollRecord> GetAllPayrollRecords()
    {
        var records = new List<PayrollRecord>();

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = new SqlCommand(
            "SELECT Id, EmployeeId, PayMonth, PayYear, GrossPay, OtherDeductions, PAYE, UIF, NetPay, ProcessedAt " +
            "FROM PayrollRecords ORDER BY ProcessedAt DESC",
            connection);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            records.Add(new PayrollRecord
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                PayMonth = reader.GetInt32(reader.GetOrdinal("PayMonth")),
                PayYear = reader.GetInt32(reader.GetOrdinal("PayYear")),
                GrossPay = reader.GetDecimal(reader.GetOrdinal("GrossPay")),
                OtherDeductions = reader.GetDecimal(reader.GetOrdinal("OtherDeductions")),
                PAYE = reader.GetDecimal(reader.GetOrdinal("PAYE")),
                UIF = reader.GetDecimal(reader.GetOrdinal("UIF")),
                NetPay = reader.GetDecimal(reader.GetOrdinal("NetPay")),
                ProcessedAt = reader.GetDateTime(reader.GetOrdinal("ProcessedAt"))
            });
        }

        return records;
    }


}