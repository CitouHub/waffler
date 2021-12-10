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