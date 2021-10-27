-- Scaffold-DbContext "Server=localhost\SQLEXPRESS02;Initial Catalog=db_waffler;persist security info=True;Integrated Security=SSPI;MultipleActiveResultSets=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir . -Context BaseDbContext -Force
-- =====================================================================
IF OBJECTPROPERTY(object_id('dbo.CandleStick'), N'IsTable') = 1 DROP TABLE [dbo].[CandleStick]
GO
IF OBJECTPROPERTY(object_id('dbo.TradeRuleCondition'), N'IsTable') = 1 DROP TABLE [dbo].[TradeRuleCondition]
GO
IF OBJECTPROPERTY(object_id('dbo.TradeOrder'), N'IsTable') = 1 DROP TABLE [dbo].[TradeOrder]
GO
IF OBJECTPROPERTY(object_id('dbo.TradeRule'), N'IsTable') = 1 DROP TABLE [dbo].[TradeRule]
GO
IF OBJECTPROPERTY(object_id('dbo.TradeType'), N'IsTable') = 1 DROP TABLE [dbo].[TradeType]
GO
IF OBJECTPROPERTY(object_id('dbo.TradeConditionOperator'), N'IsTable') = 1 DROP TABLE [dbo].[TradeConditionOperator]
GO
IF OBJECTPROPERTY(object_id('dbo.CandleStickValueType'), N'IsTable') = 1 DROP TABLE [dbo].[CandleStickValueType]
GO
IF OBJECTPROPERTY(object_id('dbo.TradeRuleConditionComparator'), N'IsTable') = 1 DROP TABLE [dbo].[TradeRuleConditionComparator]
GO
IF OBJECTPROPERTY(object_id('dbo.TradeRuleConditionSampleDirection'), N'IsTable') = 1 DROP TABLE [dbo].[TradeRuleConditionSampleDirection]
GO
IF OBJECTPROPERTY(object_id('dbo.WafflerProfile'), N'IsTable') = 1 DROP TABLE [dbo].[WafflerProfile]
GO
IF OBJECTPROPERTY(object_id('dbo.TradeOrderStatus'), N'IsTable') = 1 DROP TABLE [dbo].[TradeOrderStatus]
GO
IF OBJECTPROPERTY(object_id('dbo.TradeAction'), N'IsTable') = 1 DROP TABLE [dbo].[TradeAction]
GO

-- Create new tables
-- =====================================================================
CREATE TABLE [dbo].[TradeType](
	[Id] [smallint] IDENTITY(1,1) NOT NULL,
	[InsertDate] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
	[InsertByUser] [int] NOT NULL DEFAULT(1),
	[UpdateDate] [datetime2](7) NULL,
	[UpdateByUser] [int] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](200) NULL
 CONSTRAINT [TradeType_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

CREATE TABLE [dbo].[CandleStick](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[InsertDate] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
	[InsertByUser] [int] NOT NULL DEFAULT(1),
	[UpdateDate] [datetime2](7) NULL,
	[UpdateByUser] [int] NULL,
	[TradeTypeId] [smallint] NOT NULL,
    [HighPrice] [decimal](10,2) NOT NULL,
	[LowPrice] [decimal](10,2) NOT NULL,
	[OpenPrice] [decimal](10,2) NOT NULL,
	[ClosePrice] [decimal](10,2) NOT NULL,
	[AvgHighLowPrice] [decimal](10,2) NOT NULL,
	[AvgOpenClosePrice] [decimal](10,2) NOT NULL,
	[TotalAmount] [decimal](10,2) NOT NULL,
	[Volume] [decimal](10,2) NOT NULL,
	[PeriodDateTime] [datetime2](0) NOT NULL
 CONSTRAINT [CandleStick_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)
ALTER TABLE [dbo].[CandleStick] WITH CHECK ADD CONSTRAINT [CandleStick_TradeActionFK] FOREIGN KEY([TradeTypeId]) REFERENCES [dbo].[TradeType] ([Id])
GO

IF EXISTS (SELECT name FROM sys.indexes WHERE name = 'IdxCandleStick_PeriodDateTime')DROP INDEX [IdxCandleStick_PeriodDateTime] ON [dbo].[CandleStick]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IdxCandleStick_PeriodDateTime] ON [dbo].[CandleStick]
(
    [PeriodDateTime] ASC
) INCLUDE ([ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

CREATE TABLE [dbo].[TradeAction](
	[Id] [smallint] IDENTITY(1,1) NOT NULL,
	[InsertDate] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
	[InsertByUser] [int] NOT NULL DEFAULT(1),
	[UpdateDate] [datetime2](7) NULL,
	[UpdateByUser] [int] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](200) NULL
 CONSTRAINT [TradeAction_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

CREATE TABLE [dbo].[TradeConditionOperator](
	[Id] [smallint] IDENTITY(1,1) NOT NULL,
	[InsertDate] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
	[InsertByUser] [int] NOT NULL DEFAULT(1),
	[UpdateDate] [datetime2](7) NULL,
	[UpdateByUser] [int] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](200) NULL
 CONSTRAINT [TradeConditionOperator_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

CREATE TABLE [dbo].[TradeRule](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InsertDate] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
	[InsertByUser] [int] NOT NULL DEFAULT(1),
	[UpdateDate] [datetime2](7) NULL,
	[UpdateByUser] [int] NULL,
	[TradeActionId] [smallint] NOT NULL,
	[TradeTypeId] [smallint] NOT NULL,
	[TradeConditionOperatorId] [smallint] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](200) NULL,
	[Amount] [decimal](10,8) NOT NULL,
	[TradeMinIntervalMinutes] [int] NOT NULL,
	[LastTrigger] [datetime2](0) NOT NULL DEFAULT('1900-01-01'),
	[IsActive] [bit] NOT NULL DEFAULT(1)
 CONSTRAINT [TradeRule_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

ALTER TABLE [dbo].[TradeRule] WITH CHECK ADD CONSTRAINT [TradeRule_TradeActionFK] FOREIGN KEY([TradeActionId]) REFERENCES [dbo].[TradeAction] ([Id])
GO
ALTER TABLE [dbo].[TradeRule] WITH CHECK ADD CONSTRAINT [TradeRule_TradeTypeFK] FOREIGN KEY([TradeTypeId]) REFERENCES [dbo].[TradeType] ([Id])
GO
ALTER TABLE [dbo].[TradeRule] WITH CHECK ADD CONSTRAINT [TradeRule_TradeConditionOperatorFK] FOREIGN KEY([TradeConditionOperatorId]) REFERENCES [dbo].[TradeConditionOperator] ([Id])
GO

CREATE TABLE [dbo].[CandleStickValueType](
	[Id] [smallint] IDENTITY(1,1) NOT NULL,
	[InsertDate] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
	[InsertByUser] [int] NOT NULL DEFAULT(1),
	[UpdateDate] [datetime2](7) NULL,
	[UpdateByUser] [int] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](200) NULL
 CONSTRAINT [CandleStickValueType_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

CREATE TABLE [dbo].[TradeRuleConditionComparator](
	[Id] [smallint] IDENTITY(1,1) NOT NULL,
	[InsertDate] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
	[InsertByUser] [int] NOT NULL DEFAULT(1),
	[UpdateDate] [datetime2](7) NULL,
	[UpdateByUser] [int] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](200) NULL
 CONSTRAINT [TradeRuleConditionComparator_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

CREATE TABLE [dbo].[TradeRuleConditionSampleDirection](
	[Id] [smallint] IDENTITY(1,1) NOT NULL,
	[InsertDate] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
	[InsertByUser] [int] NOT NULL DEFAULT(1),
	[UpdateDate] [datetime2](7) NULL,
	[UpdateByUser] [int] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](200) NULL
 CONSTRAINT [TradeRuleConditionSampleDirection_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

CREATE TABLE [dbo].[TradeRuleCondition](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InsertDate] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
	[InsertByUser] [int] NOT NULL DEFAULT(1),
	[UpdateDate] [datetime2](7) NULL,
	[UpdateByUser] [int] NULL,
	[TradeRuleId] [int] NOT NULL,
	[CandleStickValueTypeId] [smallint] NOT NULL,
	[TradeRuleConditionComparatorId] [smallint] NOT NULL,
	[TradeRuleConditionSampleDirectionId] [smallint] NOT NULL,
	[FromMinutesOffset] [int] NOT NULL,
	[ToMinutesOffset] [int] NOT NULL,
	[FromMinutesSample] [int] NOT NULL,
	[ToMinutesSample] [int] NOT NULL,
	[DeltaPercent] [decimal](6,4) NOT NULL,
	[Description] [nvarchar](200) NULL,
	[IsActive] [bit] NOT NULL DEFAULT(1)
 CONSTRAINT [TradeRuleCondition_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

ALTER TABLE [dbo].[TradeRuleCondition] WITH CHECK ADD CONSTRAINT [TradeRuleCondition_TradeRuleFK] FOREIGN KEY([TradeRuleId]) REFERENCES [dbo].[TradeRule] ([ID])
GO
ALTER TABLE [dbo].[TradeRuleCondition] WITH CHECK ADD CONSTRAINT [TradeRuleCondition_CandleStickValueTypeFK] FOREIGN KEY([CandleStickValueTypeId]) REFERENCES [dbo].[CandleStickValueType] ([ID])
GO
ALTER TABLE [dbo].[TradeRuleCondition] WITH CHECK ADD CONSTRAINT [TradeRuleCondition_TradeRuleConditionComparatorFK] FOREIGN KEY([TradeRuleConditionComparatorId]) REFERENCES [dbo].[TradeRuleConditionComparator] ([ID])
GO
ALTER TABLE [dbo].[TradeRuleCondition] WITH CHECK ADD CONSTRAINT [TradeRuleCondition_TradeRuleConditionSampleDirectionFK] FOREIGN KEY(TradeRuleConditionSampleDirectionId) REFERENCES [dbo].[TradeRuleConditionSampleDirection] ([ID])
GO

CREATE TABLE [dbo].[WafflerProfile](
	[Id] [smallint] IDENTITY(1,1) NOT NULL,
	[InsertDate] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
	[InsertByUser] [int] NOT NULL DEFAULT(1),
	[UpdateDate] [datetime2](7) NULL,
	[UpdateByUser] [int] NULL,
	[Password] [nvarchar](500) NOT NULL,
	[ApiKey] [nvarchar](4000) NULL
 CONSTRAINT [WafflerProfile_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

CREATE TABLE [dbo].[TradeOrderStatus](
	[Id] [smallint] IDENTITY(1,1) NOT NULL,
	[InsertDate] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
	[InsertByUser] [int] NOT NULL DEFAULT(1),
	[UpdateDate] [datetime2](7) NULL,
	[UpdateByUser] [int] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](200) NULL
 CONSTRAINT [TradeOrderStatus_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

CREATE TABLE [dbo].[TradeOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InsertDate] [datetime2](7) NOT NULL DEFAULT(GETUTCDATE()),
	[InsertByUser] [int] NOT NULL DEFAULT(1),
	[UpdateDate] [datetime2](7) NULL,
	[UpdateByUser] [int] NULL,
	[TradeRuleId] [int] NOT NULL,
	[TradeOrderStatusId] [smallint] NOT NULL DEFAULT(1),
	[OrderId] [UNIQUEIDENTIFIER] NOT NULL,
	[OrderDateTime] [datetime2](0) NOT NULL,
	[Price] [decimal](10,2) NOT NULL,
	[Amount] [decimal](10,8) NOT NULL,
	[FilledAmount] [decimal](10,8) NOT NULL DEFAULT(0.0)
 CONSTRAINT [TradeOrder_PK] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

ALTER TABLE [dbo].[TradeOrder] WITH CHECK ADD CONSTRAINT [TradeOrder_TradeRuleFK] FOREIGN KEY([TradeRuleId]) REFERENCES [dbo].[TradeRule] ([ID])
GO
ALTER TABLE [dbo].[TradeOrder] WITH CHECK ADD CONSTRAINT [TradeOrder_TradeOrderStatusFK] FOREIGN KEY([TradeOrderStatusId]) REFERENCES [dbo].[TradeOrderStatus] ([ID])
GO