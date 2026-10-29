USE [EmployeeManagementDB]
GO
/****** Object:  Table [dbo].[Employees]    Script Date: 10/29/2026 12:41:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employees](
	[Id] [int] NOT NULL,
	[FirstName] [varchar](50) NOT NULL,
	[LastName] [varchar](50) NOT NULL,
	[Email] [varchar](100) NOT NULL,
	[Department] [varchar](50) NOT NULL,
	[Position] [varchar](50) NOT NULL,
	[Salary] [decimal](12, 2) NOT NULL,
	[HireDate] [date] NOT NULL,
	[DateOfBirth] [date] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PayrollRecords]    Script Date: 10/29/2026 12:41:21 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PayrollRecords](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[PayMonth] [int] NOT NULL,
	[PayYear] [int] NOT NULL,
	[GrossPay] [decimal](12, 2) NOT NULL,
	[OtherDeductions] [decimal](12, 2) NOT NULL,
	[PAYE] [decimal](12, 2) NOT NULL,
	[UIF] [decimal](12, 2) NOT NULL,
	[NetPay] [decimal](12, 2) NOT NULL,
	[ProcessedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Employee_PayPeriod] UNIQUE NONCLUSTERED 
(
	[EmployeeId] ASC,
	[PayMonth] ASC,
	[PayYear] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PayrollRecords]  WITH CHECK ADD  CONSTRAINT [FK_PayrollRecords_Employees] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employees] ([Id])
GO
ALTER TABLE [dbo].[PayrollRecords] CHECK CONSTRAINT [FK_PayrollRecords_Employees]
GO
