IF OBJECTPROPERTY(object_id('dbo.sp_getPriceStatistic'), N'IsProcedure') = 1 DROP PROCEDURE [dbo].[sp_getPriceStatistic]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_getPriceStatistic]
-- =====================================================================
-- Author:			Rikard Gustafsson
-- Create date:		2021-10-16
-- Description:		
-- =====================================================================
	@CandleStickValueTypeId SMALLINT,
	@PeriodDateTimeGroup SMALLINT,
	@FromPeriodDateTime DATETIME2(0),
	@ToPeriodDateTime DATETIME2(0)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @PriceStatistics TABLE(
		PriceValue DECIMAL(10,2) NOT NULL,
		PeriodDateTime DATETIME2(0) NOT NULL)

	INSERT INTO @PriceStatistics
	SELECT CASE @CandleStickValueTypeId
		WHEN 1 THEN AVG(HighPrice)
		WHEN 2 THEN AVG(LowPrice)
		WHEN 3 THEN AVG(OpenPrice)
		WHEN 4 THEN AVG(ClosePrice)
		WHEN 5 THEN AVG(AvgHighLowPrice)
		WHEN 6 THEN AVG(AvgOpenClosePrice)
		ELSE 0 END AS PriceValue,
		DATEADD(MINUTE, DATEDIFF(MINUTE, 0, PeriodDateTime) / @PeriodDateTimeGroup * @PeriodDateTimeGroup, 0) AS PeriodDateTime
	FROM CandleStick WHERE PeriodDateTime >= @FromPeriodDateTime AND PeriodDateTime <= @ToPeriodDateTime
	GROUP BY DATEADD(MINUTE, DATEDIFF(MINUTE, 0, PeriodDateTime) / @PeriodDateTimeGroup * @PeriodDateTimeGroup, 0)
	ORDER BY DATEADD(MINUTE, DATEDIFF(MINUTE, 0, PeriodDateTime) / @PeriodDateTimeGroup * @PeriodDateTimeGroup, 0) ASC

	SELECT * FROM @PriceStatistics
END
GO

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
	@PeriodDateTimeGroup SMALLINT,
	@FromPeriodDateTime DATETIME2(0),
	@ToPeriodDateTime DATETIME2(0)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @CandleSticks TABLE(
		HighPrice DECIMAL(10,2) NOT NULL,
		LowPrice DECIMAL(10,2) NOT NULL,
		OpenPrice DECIMAL(10,2) NOT NULL,
		ClosePrice DECIMAL(10,2) NOT NULL,
		Volume DECIMAL(10,2) NOT NULL,
		PeriodDateTime DATETIME2(0) NOT NULL)

	INSERT INTO @CandleSticks
	SELECT MAX(HighPrice),
		MIN(LowPrice),
		(SELECT AVG(OpenPrice) FROM CandleStick AS C WHERE C.PeriodDateTime = MIN(CS.PeriodDateTime)),
		(SELECT AVG(ClosePrice) FROM CandleStick AS C WHERE C.PeriodDateTime = MAX(CS.PeriodDateTime)),
		SUM(Volume),
		DATEADD(MINUTE, DATEDIFF(MINUTE, 0, PeriodDateTime) / @PeriodDateTimeGroup * @PeriodDateTimeGroup, 0) AS PeriodDateTime
	FROM CandleStick AS CS
	WHERE PeriodDateTime >= @FromPeriodDateTime AND PeriodDateTime <= @ToPeriodDateTime
	GROUP BY DATEADD(MINUTE, DATEDIFF(MINUTE, 0, PeriodDateTime) / @PeriodDateTimeGroup * @PeriodDateTimeGroup, 0)
	ORDER BY DATEADD(MINUTE, DATEDIFF(MINUTE, 0, PeriodDateTime) / @PeriodDateTimeGroup * @PeriodDateTimeGroup, 0) ASC

	SELECT * FROM @CandleSticks ORDER BY PeriodDateTime
END
GO
