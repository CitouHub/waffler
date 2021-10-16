DELETE TradeRuleCondition
DELETE TradeRule
DELETE TradeConditionOperator
DELETE TradeAction
DELETE CandleStickValueType
DELETE ConditionComparator

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
INSERT INTO CandleStickValueType (ID, Name) VALUES (5, 'AvgHighLow price')
INSERT INTO CandleStickValueType (ID, Name) VALUES (6, 'AvgOpenClose price')
SET IDENTITY_INSERT CandleStickValueType OFF
GO

SET IDENTITY_INSERT ConditionComparator ON
GO
INSERT INTO ConditionComparator (ID, Name) VALUES (1, 'Less then')
INSERT INTO ConditionComparator (ID, Name) VALUES (2, 'More then')
INSERT INTO ConditionComparator (ID, Name) VALUES (3, 'Abs less then')
INSERT INTO ConditionComparator (ID, Name) VALUES (4, 'Abs more then')
SET IDENTITY_INSERT ConditionComparator OFF
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
SET IDENTITY_INSERT TradeOrderStatus OFF
GO

SET IDENTITY_INSERT TradeRule ON
GO
INSERT INTO TradeRule([Id], [TradeActionId], [TradeConditionOperatorId], [Name], [Amount], [ActionMinIntervalMinutes]) VALUES (1, 1, 1, 'TestRule', 0.0005, 60*12)
SET IDENTITY_INSERT TradeRule OFF
GO

INSERT INTO TradeRuleCondition([TradeRuleId], [CandleStickValueTypeId], [ConditionComparatorId], [FromMinutesOffset], [ToMinutesOffset], [FromMinutesSample], [ToMinutesSample], [DeltaPercent], [Description]) 
VALUES (1, 1, 1, -90, -60, 15, 15, -3.0, 'A drop of over 3% over 30 minuts 90 minutes ago')
INSERT INTO TradeRuleCondition([TradeRuleId], [CandleStickValueTypeId], [ConditionComparatorId], [FromMinutesOffset], [ToMinutesOffset], [FromMinutesSample], [ToMinutesSample], [DeltaPercent], [Description]) 
VALUES (1, 1, 3, -60, 0, 15, 15, 0.5, 'No more then 0.5% change over the last 60 minutes')
INSERT INTO TradeRuleCondition([TradeRuleId], [CandleStickValueTypeId], [ConditionComparatorId], [FromMinutesOffset], [ToMinutesOffset], [FromMinutesSample], [ToMinutesSample], [DeltaPercent], [Description]) 
VALUES (1, 1, 2, -5760, 0, 15, 15, 1.0, 'The general trend over 4 days are positive')