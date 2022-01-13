IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'AvgHighLowPrice' AND Object_ID = Object_ID(N'dbo.CandleStick'))
BEGIN
    ALTER TABLE [CandleStick] DROP COLUMN [AvgHighLowPrice]
END
GO

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'AvgOpenClosePrice' AND Object_ID = Object_ID(N'dbo.CandleStick'))
BEGIN
    ALTER TABLE [CandleStick] DROP COLUMN [AvgOpenClosePrice]
END
GO