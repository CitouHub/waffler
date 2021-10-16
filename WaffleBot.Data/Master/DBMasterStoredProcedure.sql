IF OBJECTPROPERTY(object_id('dbo.sp_getPriceTrends'), N'IsProcedure') = 1 DROP PROCEDURE [dbo].[sp_getPriceTrends]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_getPriceTrends]
-- =====================================================================
-- Author:			Rikard Gustafsson
-- Create date:		2021-10-13
-- Description:		
-- =====================================================================
	@StartDateTime DATETIME2(0),
	@FromMinutesOffset INT,
	@ToMinutesOffset INT,
	@FromMinutesSample INT,
	@ToMinutesSample INT
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @FromCandleSticks TABLE(
		HighPrice DECIMAL(10,2) NOT NULL,
		LowPrice DECIMAL(10,2) NOT NULL,
		OpenPrice DECIMAL(10,2) NOT NULL,
		ClosePrice DECIMAL(10,2) NOT NULL,
		AvgHighLowPrice DECIMAL(10,2) NOT NULL,
		AvgOpenClosePrice DECIMAL(10,2) NOT NULL)

	DECLARE @ToCandleSticks TABLE(
		HighPrice DECIMAL(10,2) NOT NULL,
		LowPrice DECIMAL(10,2) NOT NULL,
		OpenPrice DECIMAL(10,2) NOT NULL,
		ClosePrice DECIMAL(10,2) NOT NULL,
		AvgHighLowPrice DECIMAL(10,2) NOT NULL,
		AvgOpenClosePrice DECIMAL(10,2) NOT NULL)

	DECLARE @FromFromDateTime DATETIME2(0) = DATEADD(MINUTE, @FromMinutesOffset - @FromMinutesSample/2, @StartDateTime)
	DECLARE @FromToDateTime DATETIME2(0) = DATEADD(MINUTE, @FromMinutesOffset + @FromMinutesSample/2, @StartDateTime)

	INSERT INTO @FromCandleSticks
	SELECT HighPrice, LowPrice, OpenPrice, ClosePrice, AvgHighLowPrice, AvgOpenClosePrice
	FROM CandleStick
	WHERE PeriodDateTime >= @FromFromDateTime
		AND PeriodDateTime <= @FromToDateTime

	DECLARE @ToFromDateTime DATETIME2(0) = DATEADD(MINUTE, @ToMinutesOffset - @ToMinutesSample/2, @StartDateTime)
	DECLARE @ToToDateTime DATETIME2(0) = DATEADD(MINUTE, @ToMinutesOffset + @ToMinutesSample/2, @StartDateTime)

	INSERT INTO @ToCandleSticks
	SELECT HighPrice, LowPrice, OpenPrice, ClosePrice, AvgHighLowPrice, AvgOpenClosePrice
	FROM CandleStick
	WHERE PeriodDateTime >= @ToFromDateTime
		AND PeriodDateTime <= @ToToDateTime

	DECLARE @Trend TABLE(
		HighPriceTrend DECIMAL(8,6) NULL,
		LowPriceTrend DECIMAL(8,6) NULL,
		OpenPriceTrend DECIMAL(8,6) NULL,
		ClosePriceTrend DECIMAL(8,6) NULL,
		AvgHighLowPriceTrend DECIMAL(8,6) NULL,
		AvgOpenClosePriceTrend DECIMAL(8,6) NULL)

	INSERT INTO @Trend
	SELECT 
		1 - F.AvgHighPrice/T.AvgHighPrice AS HighPriceTrend,
		1 - F.AvgLowPrice/T.AvgLowPrice AS LowPriceTrend,
		1 - F.AvgOpenPrice/T.AvgOpenPrice AS OpenPriceTrend,
		1 - F.AvgClosePrice/T.AvgClosePrice AS ClosePriceTrend,
		1 - F.AvgAvgHighLowPrice/T.AvgAvgHighLowPrice AS AvgHighLowPriceTrend,
		1 - F.AvgAvgOpenClosePrice/T.AvgAvgOpenClosePrice AS AvgOpenClosePriceTrend
	FROM (SELECT 
		AVG(HighPrice) AS AvgHighPrice, 
		AVG(LowPrice) AS AvgLowPrice, 
		AVG(OpenPrice) AS AvgOpenPrice, 
		AVG(ClosePrice) AS AvgClosePrice, 
		AVG(AvgHighLowPrice) AS AvgAvgHighLowPrice, 
		AVG(AvgOpenClosePrice) AS AvgAvgOpenClosePrice FROM @FromCandleSticks) AS F
		INNER JOIN (SELECT 
		AVG(HighPrice) AS AvgHighPrice, 
		AVG(LowPrice) AS AvgLowPrice, 
		AVG(OpenPrice) AS AvgOpenPrice, 
		AVG(ClosePrice) AS AvgClosePrice, 
		AVG(AvgHighLowPrice) AS AvgAvgHighLowPrice, 
		AVG(AvgOpenClosePrice) AS AvgAvgOpenClosePrice FROM @ToCandleSticks) AS T ON 1 = 1
	WHERE EXISTS (SELECT * FROM @FromCandleSticks) 
		AND EXISTS (SELECT * FROM @ToCandleSticks)

	SELECT * FROM @Trend
END
GO