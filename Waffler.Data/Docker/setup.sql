PRINT 'Using "$(RS_DATABASE_NAME)" for database name'

IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'$(RS_DATABASE_NAME)') BEGIN
	CREATE DATABASE $(RS_DATABASE_NAME);
	PRINT 'Created database'
END
ELSE PRINT 'Database existed'
GO
USE $(RS_DATABASE_NAME);
GO