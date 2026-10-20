# C# Employee Payroll System

A robust, console-based Employee Management and Payroll System built in C# and connected to a Microsoft SQL Server database using ADO.NET. Designed with a focus on defensive programming, data integrity, and South African tax compliance.

## Features
- **Employee CRUD Operations:** Add, view, update, and delete employees with local state staging and confirmation workflows.
- **Automated Payroll Processing:** Calculates South African statutory deductions including PAYE (using tax brackets and age-based rebates), UIF (with statutory caps), and net pay.
- **Database Integrity:** Enforces unique constraints at the SQL Server level to prevent duplicate payroll processing for the same employee and tax period.
- **Payroll History:** Queries and displays past payroll records formatted in South African Rands (`en-ZA`).
- **Robust Error Handling:** Comprehensive exception management for database connectivity, constraints, and user input validation.

## Tech Stack
- **Language:** C# (.NET / Object-Oriented Programming)
- **Database:** Microsoft SQL Server (MSSQL), ADO.NET (`SqlConnection`, `SqlCommand`, `SqlDataReader`)
- **Environment:** Visual Studio Code / .NET CLI