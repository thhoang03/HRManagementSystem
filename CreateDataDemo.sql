USE HRManagementSystem;
GO

INSERT INTO Departments (DepartmentName, Description) VALUES
(N'Human Resources', N'Quản lý nhân sự'),
(N'Information Technology', N'Phát triển phần mềm'),
(N'Finance', N'Quản lý tài chính'),
(N'Sales', N'Kinh doanh bán hàng'),
(N'Marketing', N'Tiếp thị sản phẩm'),
(N'Customer Support', N'Hỗ trợ khách hàng');

INSERT INTO Positions (PositionName, BaseSalary) VALUES
(N'HR Manager', 22000000),
(N'HR Staff', 15000000),
(N'Software Developer', 18000000),
(N'Senior Developer', 26000000),
(N'Accountant', 17000000),
(N'Sales Executive', 15000000),
(N'Sales Manager', 23000000),
(N'Marketing Specialist', 16000000),
(N'Support Staff', 14000000),
(N'System Administrator', 25000000);

INSERT INTO Employees
(FullName, Gender, DoB, Email, Phone, Address, HireDate, DepartmentID, PositionID)
VALUES
(N'Nguyen Van Admin', 'Male', '1990-01-01', 'admin@company.com', '0900000001', N'Hà Nội', '2020-01-10', 2, 10),

(N'Tran Thi HR', 'Female', '1993-02-10', 'hr@company.com', '0900000002', N'Hà Nội', '2021-03-12', 1, 1),

(N'Le Van Dev1', 'Male', '1996-05-20', 'dev1@company.com', '0900000003', N'Hà Nội', '2022-01-15', 2, 3),

(N'Pham Thi Dev2', 'Female', '1997-07-15', 'dev2@company.com', '0900000004', N'Hà Nội', '2023-02-20', 2, 3),

(N'Hoang Van Senior', 'Male', '1992-11-11', 'senior@company.com', '0900000005', N'Hà Nội', '2021-08-01', 2, 4),

(N'Nguyen Thi Accountant', 'Female', '1994-06-06', 'accountant@company.com', '0900000006', N'Hà Nội', '2022-04-05', 3, 5),

(N'Tran Van Sales1', 'Male', '1995-09-09', 'sales1@company.com', '0900000007', N'Hà Nội', '2022-09-10', 4, 6),

(N'Pham Van Sales2', 'Male', '1998-10-01', 'sales2@company.com', '0900000008', N'Hà Nội', '2023-05-11', 4, 6),

(N'Le Thi Marketing', 'Female', '1997-12-12', 'marketing@company.com', '0900000009', N'Hà Nội', '2023-01-01', 5, 8),

(N'Hoang Van Support1', 'Male', '1999-03-03', 'support1@company.com', '0900000010', N'Hà Nội', '2024-02-02', 6, 9),

(N'Nguyen Van Support2', 'Male', '1999-04-04', 'support2@company.com', '0900000011', N'Hà Nội', '2024-02-02', 6, 9),

(N'Tran Thi Staff1', 'Female', '1998-01-10', 'staff1@company.com', '0900000012', N'Hà Nội', '2023-02-01', 1, 2),

(N'Pham Van Dev3', 'Male', '1997-08-20', 'dev3@company.com', '0900000013', N'Hà Nội', '2022-07-01', 2, 3),

(N'Le Van Dev4', 'Male', '1996-09-22', 'dev4@company.com', '0900000014', N'Hà Nội', '2022-08-01', 2, 3),

(N'Nguyen Thi SalesManager', 'Female', '1991-01-01', 'salesmanager@company.com', '0900000015', N'Hà Nội', '2020-05-01', 4, 7);

INSERT INTO Users (EmployeeID, Username, PasswordHash, Role) VALUES
(1,'admin','123','Admin'),
(2,'hr','123','HR'),
(3,'dev1','123','Employee'),
(4,'dev2','123','Employee'),
(5,'senior','123','Manager'),
(6,'accountant','123','Employee'),
(7,'sales1','123','Employee'),
(8,'sales2','123','Employee'),
(9,'marketing','123','Employee'),
(10,'support1','123','Employee'),
(11,'support2','123','Employee'),
(12,'hrstaff','123','HR'),
(13,'dev3','123','Employee'),
(14,'dev4','123','Employee'),
(15,'salesmanager','123','Manager');

INSERT INTO Contracts (EmployeeID, ContractType, StartDate, ContractSalary)
VALUES
(1,'Full-time','2020-01-10',25000000),
(2,'Full-time','2021-03-12',22000000),
(3,'Full-time','2022-01-15',18000000),
(4,'Full-time','2023-02-20',18000000),
(5,'Full-time','2021-08-01',26000000),
(6,'Full-time','2022-04-05',17000000),
(7,'Full-time','2022-09-10',15000000),
(8,'Full-time','2023-05-11',15000000),
(9,'Full-time','2023-01-01',16000000),
(10,'Full-time','2024-02-02',14000000),
(11,'Full-time','2024-02-02',14000000),
(12,'Full-time','2023-02-01',15000000),
(13,'Full-time','2022-07-01',18000000),
(14,'Full-time','2022-08-01',18000000),
(15,'Full-time','2020-05-01',23000000);

INSERT INTO LeaveRequests (EmployeeID, StartDate, EndDate, LeaveType, Reason, Status, ApprovedBy)
VALUES
(3,'2026-03-20','2026-03-21','Annual Leave',N'Đi du lịch','Approved',2),
(4,'2026-03-18','2026-03-18','Sick Leave',N'Bị ốm','Pending',NULL),
(7,'2026-03-25','2026-03-25','Personal Leave',N'Việc gia đình','Approved',2);

INSERT INTO Payroll
(ContractID,EmployeeID,Month,Year,BaseSalary,OvertimePay,Deduction,Bonus,NetSalary,Status)
VALUES
(3,3,3,2026,18000000,2000000,500000,1000000,20500000,'Approved'),
(4,4,3,2026,18000000,1000000,300000,500000,19200000,'Approved'),
(5,5,3,2026,26000000,2000000,800000,2000000,29200000,'Paid'),
(7,7,3,2026,15000000,500000,200000,300000,15600000,'Draft');
