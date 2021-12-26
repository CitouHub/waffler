USE [Waffler]
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
		Volume DECIMAL(10,2) NOT NULL,
		PeriodDateTime DATETIME2(0) NOT NULL)

	INSERT INTO @CandleSticks
	SELECT MAX(HighPrice),
		MIN(LowPrice),
		(SELECT AVG(OpenPrice) FROM CandleStick AS CS WHERE CS.PeriodDateTime = MIN(CandleStick.PeriodDateTime)),
		(SELECT AVG(ClosePrice) FROM CandleStick AS CS WHERE CS.PeriodDateTime = MAX(CandleStick.PeriodDateTime)),
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

IF OBJECTPROPERTY(object_id('dbo.sp_getTradeOrders'), N'IsProcedure') = 1 DROP PROCEDURE [dbo].[sp_getTradeOrders]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_getTradeOrders]
-- =====================================================================
-- Author:			Rikard Gustafsson
-- Create date:		2021-10-27
-- Description:		
-- =====================================================================
	@FromPeriodDateTime DATETIME2(0),
	@ToPeriodDateTime DATETIME2(0)
AS
BEGIN
	SET NOCOUNT ON

	SELECT 
		TradeOrder.Id AS Id,
		ISNULL(TradeRule.Id, 0) AS TradeRuleId,
		ISNULL(TradeRule.Name, 'Manual') AS TradeRuleName,
		TradeAction.Id AS TradeActionId,
		TradeAction.Name AS TradeActionName,
		TradeOrderStatus.Id AS TradeOrderStatusId,
		TradeOrderStatus.Name AS TradeOrderStatusName,
		TradeOrder.OrderId AS OrderId,
		TradeOrder.OrderDateTime AS OrderDateTime,
		TradeOrder.Price AS Price,
		TradeOrder.Amount AS Amount,
		TradeOrder.FilledAmount AS FilledAmount
	FROM TradeOrder
		LEFT JOIN TradeRule ON TradeOrder.TradeRuleId = TradeRule.Id
		INNER JOIN TradeAction ON TradeOrder.TradeActionId = TradeAction.Id
		INNER JOIN TradeOrderStatus ON TradeOrder.TradeOrderStatusId = TradeOrderStatus.Id
	WHERE TradeOrder.OrderDateTime >= @FromPeriodDateTime
		AND TradeOrder.OrderDateTime <= @ToPeriodDateTime
END
GO

IF OBJECTPROPERTY(object_id('dbo.sp_getTradeRuleBuyStatistics'), N'IsProcedure') = 1 DROP PROCEDURE [dbo].[sp_getTradeRuleBuyStatistics]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_getTradeRuleBuyStatistics]
-- =====================================================================
-- Author:			Rikard Gustafsson
-- Create date:		2021-12-26
-- Description:		
-- =====================================================================
	@FromPeriodDateTime DATETIME2(0),
	@ToPeriodDateTime DATETIME2(0),
	@StatisticsMode SMALLINT
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @CurrentPrice DECIMAL(10,2)
	SET @CurrentPrice = (SELECT TOP 1 ClosePrice FROM CandleStick ORDER BY PeriodDateTime DESC)

	DECLARE @IncludedStatuses TABLE(TradeOrderStatusId SMALLINT NOT NULL)

	IF(@StatisticsMode = 1 OR @StatisticsMode = 3) 
	BEGIN
		INSERT INTO @IncludedStatuses SELECT Id FROM TradeOrderStatus
		WHERE Id <= 6
	END

	IF(@StatisticsMode = 2 OR @StatisticsMode = 3) 
	BEGIN
		INSERT INTO @IncludedStatuses VALUES(10)
	END

	SELECT ISNULL(TradeRuleId, 0) AS TradeRuleId,
		ISNULL(TradeRule.Name, 'Manual') AS TradeRuleName,
		COUNT(*) AS Orders,
		ROUND(SUM(TradeOrder.Amount), 8) AS TotalAmount,
		ROUND(SUM(FilledAmount), 8) AS TotalFilledAmount,
		CAST(ROUND((SUM(FilledAmount) / SUM(TradeOrder.Amount)) * 100, 2) AS DECIMAL(5,2)) AS FilledPercent,
		CAST(ROUND(SUM(FilledAmount * Price), 2) AS DECIMAL(10, 2)) AS TotalInvested,
		CAST(ROUND(SUM(FilledAmount * Price) / SUM(FilledAmount), 2) AS DECIMAL(10,2)) AS AveragePrice,
		CAST(ROUND((@CurrentPrice / (SUM(FilledAmount * Price) / SUM(FilledAmount)) - 1) * 100, 2) AS DECIMAL(5,2)) AS ValueIncrease
	FROM TradeOrder 
		LEFT JOIN TradeRule ON TradeOrder.TradeRuleId = TradeRule.Id
		INNER JOIN @IncludedStatuses AS IncludedStatus ON TradeOrder.TradeOrderStatusId = IncludedStatus.TradeOrderStatusId
	WHERE TradeOrder.TradeActionId = 1
		AND TradeOrder.OrderDateTime >= @FromPeriodDateTime
		AND TradeOrder.OrderDateTime <= @ToPeriodDateTime 
	GROUP BY ISNULL(TradeRuleId, 0),
		ISNULL(TradeRule.Name, 'Manual')
	ORDER BY ISNULL(TradeRuleId, 0) ASC
END
GO