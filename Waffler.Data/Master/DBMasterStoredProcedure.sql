IF OBJECTPROPERTY(object_id('dbo.sp_getCandleSticks'), N'IsProcedure') = 1 DROP PROCEDURE [dbo].[sp_getCandleSticks]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_getCandleSticks]
-- =====================================================================
-- Author:			Rikard Gustafsson
-- Create date:		2021-10-16
-- Description:		
-- =====================================================================
	@FromPeriodDateTime DATETIME2(0),
	@ToPeriodDateTime DATETIME2(0),
	@TradeTypeId SMALLINT,
	@PeriodDateTimeGroup SMALLINT
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @CandleSticks TABLE(
		HighPrice DECIMAL(10,2) NOT NULL,
		LowPrice DECIMAL(10,2) NOT NULL,
		OpenPrice DECIMAL(10,2) NOT NULL,
		ClosePrice DECIMAL(10,2) NOT NULL,
		AvgHighLowPrice DECIMAL(10,2) NOT NULL,
		AvgOpenClosePrice DECIMAL(10,2) NOT NULL,
		Volume DECIMAL(10,2) NOT NULL,
		PeriodDateTime DATETIME2(0) NOT NULL)

	INSERT INTO @CandleSticks
	SELECT MAX(HighPrice),
		MIN(LowPrice),
		(SELECT AVG(OpenPrice) FROM CandleStick AS CS WHERE CS.PeriodDateTime = MIN(CandleStick.PeriodDateTime)),
		(SELECT AVG(ClosePrice) FROM CandleStick AS CS WHERE CS.PeriodDateTime = MAX(CandleStick.PeriodDateTime)),
		AVG(AvgHighLowPrice),
		AVG(AvgOpenClosePrice),
		SUM(Volume),
		DATEADD(MINUTE, (DATEDIFF(MINUTE, 0, PeriodDateTime) / @PeriodDateTimeGroup + 1) * @PeriodDateTimeGroup, 0) AS PeriodDateTime
	FROM CandleStick
	WHERE CandleStick.TradeTypeId = 1
		AND CandleStick.PeriodDateTime >= @FromPeriodDateTime 
		AND CandleStick.PeriodDateTime <= @ToPeriodDateTime
	GROUP BY DATEADD(MINUTE, (DATEDIFF(MINUTE, 0, PeriodDateTime) / @PeriodDateTimeGroup + 1)* @PeriodDateTimeGroup, 0)
	ORDER BY DATEADD(MINUTE, (DATEDIFF(MINUTE, 0, PeriodDateTime) / @PeriodDateTimeGroup + 1)* @PeriodDateTimeGroup, 0) ASC

	SELECT * FROM @CandleSticks ORDER BY PeriodDateTime
END
GO