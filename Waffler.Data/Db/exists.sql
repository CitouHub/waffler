IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'$(RS_DATABASE_NAME)') 
BEGIN
	PRINT 'TRUE'
END
ELSE 
BEGIN
	PRINT 'FALSE'
END
GO