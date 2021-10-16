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
	@FromFromDateTime DATETIME2(0),
	@FromToDateTime DATETIME2(0),
	@ToFromDateTime DATETIME2(0),
	@ToToDateTime DATETIME2(0)
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

	INSERT INTO @FromCandleSticks
	SELECT HighPrice, LowPrice, OpenPrice, ClosePrice, AvgHighLowPrice, AvgOpenClosePrice
	FROM CandleStick
	WHERE PeriodDateTime >= @FromFromDateTime
		AND PeriodDateTime <= @FromToDateTime

	INSERT INTO @ToCandleSticks
	SELECT HighPrice, LowPrice, OpenPrice, ClosePrice, AvgHighLowPrice, AvgOpenClosePrice
	FROM CandleStick
	WHERE PeriodDateTime >= @ToFromDateTime
		AND PeriodDateTime <= @ToToDateTime

	DECLARE @Trend TABLE(
		HighPriceTrend DECIMAL(6,4) NULL,
		LowPriceTrend DECIMAL(6,4) NULL,
		OpenPriceTrend DECIMAL(6,4) NULL,
		ClosePriceTrend DECIMAL(6,4) NULL,
		HighLowPriceTrend DECIMAL(6,4) NULL,
		OpenClosePriceTrend DECIMAL(6,4) NULL,
		AvgHighLowPriceTrend DECIMAL(6,4) NULL,
		AvgOpenClosePriceTrend DECIMAL(6,4) NULL)

	INSERT INTO @Trend
	SELECT 
		ROUND((1 - F.AvgHighPrice/T.AvgHighPrice)*100, 4) AS HighPriceTrend,
		ROUND((1 - F.AvgLowPrice/T.AvgLowPrice)*100, 4) AS LowPriceTrend,
		ROUND((1 - F.AvgOpenPrice/T.AvgOpenPrice)*100, 4) AS OpenPriceTrend,
		ROUND((1 - F.AvgClosePrice/T.AvgClosePrice)*100, 4) AS ClosePriceTrend,
		ROUND((1 - F.AvgHighPrice/T.AvgLowPrice)*100, 4) AS HighLowPriceTrend,
		ROUND((1 - F.AvgOpenPrice/T.AvgClosePrice)*100, 4) AS OpenClosePriceTrend,
		ROUND((1 - F.AvgAvgHighLowPrice/T.AvgAvgHighLowPrice)*100, 4) AS AvgHighLowPriceTrend,
		ROUND((1 - F.AvgAvgOpenClosePrice/T.AvgAvgOpenClosePrice)*100, 4) AS AvgOpenClosePriceTrend
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