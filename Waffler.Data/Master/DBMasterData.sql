USE [Waffler]
GO

DELETE CandleStick
DELETE TradeOrder
DELETE TradeRuleCondition
DELETE TradeRule
DELETE TradeType
DELETE TradeConditionOperator
DELETE TradeRuleStatus
DELETE TradeAction
DELETE CandleStickValueType
DELETE TradeRuleConditionComparator
DELETE TradeRuleConditionPeriodDirection
DELETE TradeOrderStatus

SET IDENTITY_INSERT TradeConditionOperator ON
GO
INSERT INTO TradeConditionOperator (ID, Name) VALUES (1, 'AND')
INSERT INTO TradeConditionOperator (ID, Name) VALUES (2, 'OR')
SET IDENTITY_INSERT TradeConditionOperator OFF
GO

SET IDENTITY_INSERT TradeAction ON
GO
INSERT INTO TradeAction (ID, Name) VALUES (1, 'Buy')
INSERT INTO TradeAction (ID, Name) VALUES (2, 'Sell')
SET IDENTITY_INSERT TradeAction OFF
GO

SET IDENTITY_INSERT TradeType ON
GO
INSERT INTO TradeType (ID, Name) VALUES (1, 'BTC_EUR')
SET IDENTITY_INSERT TradeType OFF
GO

SET IDENTITY_INSERT CandleStickValueType ON
GO
INSERT INTO CandleStickValueType (ID, Name) VALUES (1, 'High price')
INSERT INTO CandleStickValueType (ID, Name) VALUES (2, 'Low price')
INSERT INTO CandleStickValueType (ID, Name) VALUES (3, 'Open price')
INSERT INTO CandleStickValueType (ID, Name) VALUES (4, 'Close price')
SET IDENTITY_INSERT CandleStickValueType OFF
GO

SET IDENTITY_INSERT TradeRuleConditionComparator ON
GO
INSERT INTO TradeRuleConditionComparator (ID, Name) VALUES (1, 'Less then')
INSERT INTO TradeRuleConditionComparator (ID, Name) VALUES (2, 'More then')
INSERT INTO TradeRuleConditionComparator (ID, Name) VALUES (3, 'Abs less then')
INSERT INTO TradeRuleConditionComparator (ID, Name) VALUES (4, 'Abs more then')
SET IDENTITY_INSERT TradeRuleConditionComparator OFF
GO

SET IDENTITY_INSERT TradeRuleStatus ON
GO
INSERT INTO TradeRuleStatus (ID, Name) VALUES (1, 'Inactive')
INSERT INTO TradeRuleStatus (ID, Name) VALUES (2, 'Test')
INSERT INTO TradeRuleStatus (ID, Name) VALUES (3, 'Active')
SET IDENTITY_INSERT TradeRuleStatus OFF
GO

SET IDENTITY_INSERT TradeRuleConditionPeriodDirection ON
GO
INSERT INTO TradeRuleConditionPeriodDirection (ID, Name, Description) VALUES (1, 'Centered', 'The FromDate is sampled left and right from the sample point')
INSERT INTO TradeRuleConditionPeriodDirection (ID, Name, Description) VALUES (2, 'Left shift', 'The FromDate is sampled left from the sample point')
INSERT INTO TradeRuleConditionPeriodDirection (ID, Name, Description) VALUES (3, 'Right shift', 'The FromDate is sampled right from the sample point')
SET IDENTITY_INSERT TradeRuleConditionPeriodDirection OFF
GO

SET IDENTITY_INSERT TradeOrderStatus ON
GO
INSERT INTO TradeOrderStatus (ID, Name) VALUES (1, 'Open')
INSERT INTO TradeOrderStatus (ID, Name) VALUES (2, 'StopTriggered')
INSERT INTO TradeOrderStatus (ID, Name) VALUES (3, 'Filled')
INSERT INTO TradeOrderStatus (ID, Name) VALUES (4, 'FilledFully')
INSERT INTO TradeOrderStatus (ID, Name) VALUES (5, 'FilledClosed')
INSERT INTO TradeOrderStatus (ID, Name) VALUES (6, 'FilledRejected')
INSERT INTO TradeOrderStatus (ID, Name) VALUES (7, 'Rejected')
INSERT INTO TradeOrderStatus (ID, Name) VALUES (8, 'Closed')
INSERT INTO TradeOrderStatus (ID, Name) VALUES (9, 'Failed')
INSERT INTO TradeOrderStatus (ID, Name) VALUES (10, 'Test')
SET IDENTITY_INSERT TradeOrderStatus OFF
GO

SET IDENTITY_INSERT TradeRule ON
GO
INSERT INTO TradeRule([Id], [TradeActionId], [TradeTypeId], [TradeConditionOperatorId], [TradeRuleStatusId], [CandleStickValueTypeId], [Name], [Amount], [TradeOrderExpirationMinutes], [TradeMinIntervalMinutes]) VALUES (1, 1, 1, 1, 2, 4, 'Fast drop, small buy', 10, 60, 60*2)
INSERT INTO TradeRule([Id], [TradeActionId], [TradeTypeId], [TradeConditionOperatorId], [TradeRuleStatusId], [CandleStickValueTypeId], [Name], [Amount], [TradeOrderExpirationMinutes], [TradeMinIntervalMinutes]) VALUES (2, 1, 1, 1, 2, 4, 'Slow drop, big buy', 40, 60, 60*6)
INSERT INTO TradeRule([Id], [TradeActionId], [TradeTypeId], [TradeConditionOperatorId], [TradeRuleStatusId], [CandleStickValueTypeId], [Name], [Amount], [TradeOrderExpirationMinutes], [TradeMinIntervalMinutes]) VALUES (3, 1, 1, 2, 2, 4, 'Buy weekly', 20, 60, 7*24*60)
SET IDENTITY_INSERT TradeRule OFF
GO

INSERT INTO TradeRuleCondition([TradeRuleId], [TradeRuleConditionComparatorId], [DeltaPercent], [Description], [IsOn],
    [FromCandleStickValueTypeId], [FromTradeRuleConditionPeriodDirectionId], [FromMinutes], [FromPeriodMinutes],
    [ToCandleStickValueTypeId], [ToTradeRuleConditionPeriodDirectionId], [ToMinutes], [ToPeriodMinutes]) 
VALUES (1, 1, -3, 'A drop of over 3% now', 1,
	3, 2, -3*60, 60, 
	4, 2, 0, 60)

INSERT INTO TradeRuleCondition([TradeRuleId], [TradeRuleConditionComparatorId], [DeltaPercent], [Description], [IsOn],
    [FromCandleStickValueTypeId], [FromTradeRuleConditionPeriodDirectionId], [FromMinutes], [FromPeriodMinutes],
    [ToCandleStickValueTypeId], [ToTradeRuleConditionPeriodDirectionId], [ToMinutes], [ToPeriodMinutes]) 
VALUES (2, 1, -3, 'A drop of over 6% over 4 days ago', 1,
	3, 1, -4*24*60, 6*60, 
	4, 2, 0, 6*60)

INSERT INTO TradeRuleCondition([TradeRuleId], [TradeRuleConditionComparatorId], [DeltaPercent], [Description], [IsOn],
    [FromCandleStickValueTypeId], [FromTradeRuleConditionPeriodDirectionId], [FromMinutes], [FromPeriodMinutes],
    [ToCandleStickValueTypeId], [ToTradeRuleConditionPeriodDirectionId], [ToMinutes], [ToPeriodMinutes]) 
VALUES (3, 4, -1, 'Any change', 1,
	1, 1, -60, 30, 
	1, 2, 0, 30)