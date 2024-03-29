﻿USE [Waffler]
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
		DATEADD(MINUTE, (DATEDIFF(MINUTE, @FromPeriodDateTime, PeriodDateTime) / @PeriodDateTimeGroup + 1) * @PeriodDateTimeGroup, @FromPeriodDateTime) AS PeriodDateTime
	FROM CandleStick
	WHERE CandleStick.TradeTypeId = @TradeTypeId
		AND CandleStick.PeriodDateTime >= @FromPeriodDateTime 
		AND CandleStick.PeriodDateTime <= @ToPeriodDateTime
	GROUP BY DATEADD(MINUTE, (DATEDIFF(MINUTE, @FromPeriodDateTime, PeriodDateTime) / @PeriodDateTimeGroup + 1) * @PeriodDateTimeGroup, @FromPeriodDateTime)

	SELECT * FROM @CandleSticks ORDER BY PeriodDateTime
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
	@TradeTypeId SMALLINT,
	@TradeRules NVARCHAR(50),
	@TradeOrderStatuses NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @CurrentPrice DECIMAL(10,2)
	SET @CurrentPrice = (SELECT TOP 1 ClosePrice FROM CandleStick ORDER BY PeriodDateTime DESC)

	DECLARE @TradeRuleIds TABLE(Id SMALLINT NOT NULL) INSERT INTO @TradeRuleIds SELECT Id FROM dbo.tf_getIds(@TradeRules)
	DECLARE @TradeOrderStatuseIds TABLE(Id SMALLINT NOT NULL) INSERT INTO @TradeOrderStatuseIds SELECT Id FROM dbo.tf_getIds(@TradeOrderStatuses)

	SELECT TradeRuleId AS TradeRuleId,
		TradeRule.Name AS TradeRuleName,
		TradeRule.IsDeleted AS TradeRuleIsDeleted,
		COUNT(*) AS Orders,
		ROUND(SUM(TradeOrder.Amount), 8) AS TotalAmount,
		ROUND(SUM(FilledAmount), 8) AS TotalFilledAmount,
		CAST(ROUND((SUM(FilledAmount) / SUM(TradeOrder.Amount)) * 100, 2) AS DECIMAL(5,2)) AS FilledPercent,
		CAST(ROUND(SUM(FilledAmount * Price), 2) AS DECIMAL(10, 2)) AS TotalInvested,
		CAST(ROUND(SUM(FilledAmount * Price) / (CASE WHEN SUM(FilledAmount) > 0 THEN SUM(FilledAmount) ELSE CAST(COUNT(*) AS DECIMAL(10,2)) END) , 2) AS DECIMAL(10,2)) AS AveragePrice,
		CASE WHEN SUM(FilledAmount) > 0 THEN CAST(ROUND((@CurrentPrice / (SUM(FilledAmount * Price) / SUM(FilledAmount)) - 1) * 100, 2) AS DECIMAL(5,2)) ELSE 0 END AS ValueIncrease
	FROM TradeOrder
		LEFT JOIN TradeRule ON TradeOrder.TradeRuleId = TradeRule.Id
		INNER JOIN @TradeRuleIds AS IncludedTradeRule ON ISNULL(TradeOrder.TradeRuleId, 0) = IncludedTradeRule.Id
		INNER JOIN @TradeOrderStatuseIds AS IncludedTradeOrderStatuse ON TradeOrder.TradeOrderStatusId = IncludedTradeOrderStatuse.Id
	WHERE TradeOrder.TradeActionId = @TradeTypeId
		AND TradeOrder.OrderDateTime >= @FromPeriodDateTime
		AND TradeOrder.OrderDateTime <= @ToPeriodDateTime 
	GROUP BY TradeRuleId,
		TradeRule.Name,
		TradeRule.IsDeleted
	ORDER BY ISNULL(TradeRuleId, 0) ASC
END
GO

IF OBJECTPROPERTY(object_id('dbo.sp_getIndexFragmentation'), N'IsProcedure') = 1 DROP PROCEDURE [dbo].[sp_getIndexFragmentation]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_getIndexFragmentation]
-- =====================================================================
-- Author:			Rikard Gustafsson
-- Create date:		2022-01-03
-- Description:		
-- =====================================================================
AS
BEGIN
	SET NOCOUNT ON

	SELECT S.name AS [SchemaName],
		T.name as [TableName],
		I.name as [IndexName],
		CAST(ROUND(DDIPS.avg_fragmentation_in_percent, 2) AS DECIMAL(5,2)) AS [Fragmentation],
		DDIPS.page_count AS [PageCount]
	FROM sys.dm_db_index_physical_stats (DB_ID(), NULL, NULL, NULL, NULL) AS DDIPS
		INNER JOIN sys.tables T on T.object_id = DDIPS.object_id
		INNER JOIN sys.schemas S on T.schema_id = S.schema_id
		INNER JOIN sys.indexes I ON I.object_id = DDIPS.object_id
		AND DDIPS.index_id = I.index_id
	WHERE DDIPS.database_id = DB_ID()
END
GO

IF OBJECTPROPERTY(object_id('dbo.tf_getIds'), N'IsTableFunction') = 1 DROP FUNCTION [dbo].[tf_getIds]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =====================================================================
-- Author:			Rikard Gustafsson
-- Create date:		2022-01-15
-- Description:		Takes a ";"-seperated list of Ids and returns
--					a table with all the Ids separated.
-- =====================================================================
CREATE FUNCTION [dbo].[tf_getIds](
	@IdString NVARCHAR(MAX))
RETURNS @Ids TABLE (Id INT NOT NULL) 
AS
BEGIN
	WHILE LEN(@IdString) > 0
	BEGIN
		DECLARE @Id NVARCHAR(50)

		IF (PATINDEX('%;%', @IdString) > 0)
		BEGIN
			SET @Id = SUBSTRING(@IdString, 0, PATINDEX('%;%', @IdString))
			SET @IdString = SUBSTRING(@IdString, LEN(@Id + ';') + 1, LEN(@IdString))
		END
		ELSE
		BEGIN
			SET @Id = @IdString
			SET @IdString = NULL
		END

		INSERT INTO @Ids SELECT CONVERT(BIGINT, @Id)
	END

	RETURN
END
GO