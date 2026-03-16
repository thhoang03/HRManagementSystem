CREATE DATABASE HRManagementSystem;
GO

USE HRManagementSystem;
GO


CREATE TABLE Departments (
    DepartmentID INT PRIMARY KEY IDENTITY(1,1),
    DepartmentName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255)
);

CREATE TABLE Positions (
    PositionID INT PRIMARY KEY IDENTITY(1,1),
    PositionName NVARCHAR(100) NOT NULL,
    BaseSalary DECIMAL(18,2)
);

CREATE TABLE Employees (
    EmployeeID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Gender NVARCHAR(10),
    DoB DATE,
    Email VARCHAR(100) UNIQUE,
    Phone VARCHAR(20),
    Address NVARCHAR(255),
    HireDate DATE,
    DepartmentID INT,
    PositionID INT,
    Status NVARCHAR(50) DEFAULT 'Active',

    FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID),
    FOREIGN KEY (PositionID) REFERENCES Positions(PositionID)
);

CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT UNIQUE,
    Username VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role NVARCHAR(20) DEFAULT 'Employee',	-- Admin, HR, Manager, Employee
    LastLogin DATETIME,

    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);

CREATE TABLE Contracts (
    ContractID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT,
    ContractType NVARCHAR(50),
    StartDate DATE NOT NULL,
    EndDate DATE,
    ContractSalary DECIMAL(18,2),
    Status NVARCHAR(20) DEFAULT 'Valid',

    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),

    CHECK (EndDate IS NULL OR EndDate >= StartDate)
);

CREATE TABLE Attendance (
    AttendanceID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT,
    AttendanceDate DATE DEFAULT CAST(GETDATE() AS DATE),
    CheckIn DATETIME,
    CheckOut DATETIME,
    DeviceIP VARCHAR(50),
    Status NVARCHAR(50),	--Ontime, Late, EarlyLeave, Absent
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),

    UNIQUE(EmployeeID, AttendanceDate)
);

CREATE TABLE LeaveRequests (
    LeaveID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    LeaveType NVARCHAR(50),
    Reason NVARCHAR(255),
    Status NVARCHAR(20) DEFAULT 'Pending',	--Pending, Approved, Rejected
    ApprovedBy INT,

    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),
    FOREIGN KEY (ApprovedBy) REFERENCES Employees(EmployeeID)
);

CREATE TABLE Payroll (
    PayrollID INT PRIMARY KEY IDENTITY(1,1),
	ContractID INT,
    EmployeeID INT,
    Month INT,
    Year INT,
    BaseSalary DECIMAL(18,2),
    OvertimePay DECIMAL(18,2) DEFAULT 0,
    Deduction DECIMAL(18,2) DEFAULT 0,
    Bonus DECIMAL(18,2) DEFAULT 0,
    NetSalary DECIMAL(18,2),
	Status NVARCHAR(50),	--Draft, Approved, Paid
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),
	FOREIGN KEY (ContractID) REFERENCES Contracts(ContractID)
);

CREATE TABLE Settings (
    SettingID INT PRIMARY KEY IDENTITY(1,1),
    SettingKey VARCHAR(50) UNIQUE NOT NULL,
    SettingValue NVARCHAR(255) NOT NULL,
    Description NVARCHAR(255)
);


INSERT INTO Settings (SettingKey, SettingValue, Description) VALUES
('WORK_START_TIME', '08:00:00', N'Giờ bắt đầu làm việc'),
('WORK_END_TIME', '17:00:00', N'Giờ kết thúc làm việc'),
('LATE_GRACE_MINUTES', '5', N'Số phút cho phép đi muộn'),
('INSURANCE_RATE', '0.08', N'Tỷ lệ đóng bảo hiểm');