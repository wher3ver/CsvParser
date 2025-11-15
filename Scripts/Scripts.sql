IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CabData')
BEGIN
    CREATE DATABASE CabData;
END
GO

USE CabData;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CabData')
BEGIN
    CREATE TABLE CabData (
        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
        TpepPickupDatetime DATETIME2(0) NOT NULL,
        TpepDropoffDatetime DATETIME2(0) NOT NULL,
        PassengerCount TINYINT NOT NULL,
        TripDistance DECIMAL(8,2) NOT NULL,
        StoreAndFwdFlag VARCHAR(3) NOT NULL,
        PULocationID SMALLINT NOT NULL,
        DOLocationID SMALLINT NOT NULL,
        FareAmount DECIMAL(10,2) NOT NULL,
        TipAmount DECIMAL(10,2) NOT NULL,
        
        CONSTRAINT UQ_CabData UNIQUE (TpepPickupDatetime, TpepDropoffDatetime, PassengerCount)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CabData_PULocationID_TipAmount')
BEGIN
    CREATE NONCLUSTERED INDEX IX_CabData_PULocationID_TipAmount 
    ON CabData (PULocationID, TipAmount DESC);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CabData_TripDistance')
BEGIN
    CREATE NONCLUSTERED INDEX IX_CabData_TripDistance 
    ON CabData (TripDistance DESC);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CabData_Datetime')
BEGIN
    CREATE NONCLUSTERED INDEX IX_CabData_Datetime 
    ON CabData (TpepPickupDatetime, TpepDropoffDatetime);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CabData_PULocationID')
BEGIN
    CREATE NONCLUSTERED INDEX IX_CabData_PULocationID 
    ON CabData (PULocationID);
END
GO