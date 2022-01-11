IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'TradeOrderSyncStatus'))
BEGIN
    CREATE TABLE [dbo].[TradeOrderSyncStatus](
		[Id] [smallint] IDENTITY(1,1) NOT NULL,
		[InsertDate] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
		[InsertByUser] [int] NOT NULL DEFAULT(1),
		[UpdateDate] [datetime2](7) NULL,
		[UpdateByUser] [int] NULL,
		[CurrentPosition] [datetime2](0) NULL,
	 CONSTRAINT [TradeOrderSyncStatus_PK] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)

	INSERT INTO [TradeOrderSyncStatus] (CurrentPosition)
	SELECT [CandleStickSyncFromDate] FROM WafflerProfile
END