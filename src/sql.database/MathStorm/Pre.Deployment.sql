-- Pre-deployment script: ensure schema exists before objects are created
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'mathstorm')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA [mathstorm]';
END
GO
