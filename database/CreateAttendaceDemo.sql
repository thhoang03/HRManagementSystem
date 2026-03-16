USE HRManagementSystem;
GO

DECLARE @StartDate DATE = DATEADD(DAY, -29, CAST(GETDATE() AS DATE));
DECLARE @EndDate DATE = CAST(GETDATE() AS DATE);

DECLARE @CurrentDate DATE = @StartDate;

WHILE @CurrentDate <= @EndDate
BEGIN

    INSERT INTO Attendance (EmployeeID, AttendanceDate, CheckIn, CheckOut, DeviceIP, Status)
    SELECT 
        e.EmployeeID,
        @CurrentDate,

        CASE 
            WHEN r.Status = 'Absent' THEN NULL
            ELSE DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 20, DATEADD(HOUR, 8, CAST(@CurrentDate AS DATETIME)))
        END,

        CASE 
            WHEN r.Status = 'Absent' THEN NULL
            ELSE DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 20, DATEADD(HOUR, 17, CAST(@CurrentDate AS DATETIME)))
        END,

        '192.168.1.' + CAST((ABS(CHECKSUM(NEWID())) % 50 + 1) AS VARCHAR),

        r.Status

    FROM Employees e
    CROSS APPLY (
        SELECT CASE 
            WHEN RAND(CHECKSUM(NEWID())) < 0.1 THEN 'Absent'
            WHEN RAND(CHECKSUM(NEWID())) < 0.2 THEN 'Late'
            ELSE 'Ontime'
        END AS Status
    ) r

    WHERE NOT EXISTS (
        SELECT 1 
        FROM Attendance a
        WHERE a.EmployeeID = e.EmployeeID
        AND a.AttendanceDate = @CurrentDate
    );

    SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);

END