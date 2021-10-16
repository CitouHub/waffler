DELETE CandleStick
DELETE TradeOrder
DELETE TradeRuleCondition
DELETE TradeRule
DELETE TradeType
DELETE TradeConditionOperator
DELETE TradeAction
DELETE CandleStickValueType
DELETE TradeRuleConditionComparator
DELETE TradeRuleConditionSampleDirection
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
INSERT INTO CandleStickValueType (ID, Name) VALUES (5, 'HighLow price')
INSERT INTO CandleStickValueType (ID, Name) VALUES (6, 'OpenClose price')
INSERT INTO CandleStickValueType (ID, Name) VALUES (7, 'AvgHighLow price')
INSERT INTO CandleStickValueType (ID, Name) VALUES (8, 'AvgOpenClose price')
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

SET IDENTITY_INSERT TradeRuleConditionSampleDirection ON
GO
INSERT INTO TradeRuleConditionSampleDirection (ID, Name) VALUES (1, 'Inward')
INSERT INTO TradeRuleConditionSampleDirection (ID, Name) VALUES (2, 'Outward')
INSERT INTO TradeRuleConditionSampleDirection (ID, Name) VALUES (3, 'Left shift')
INSERT INTO TradeRuleConditionSampleDirection (ID, Name) VALUES (4, 'Right shift')
INSERT INTO TradeRuleConditionSampleDirection (ID, Name) VALUES (5, 'Centered')
SET IDENTITY_INSERT TradeRuleConditionSampleDirection OFF
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
INSERT INTO TradeRule([Id], [TradeActionId], [TradeTypeId], [TradeConditionOperatorId], [Name], [Amount], [TradeMinIntervalMinutes]) VALUES (1, 1, 1, 1, 'TestRule', 0.0005, 60*4)
SET IDENTITY_INSERT TradeRule OFF
GO

INSERT INTO TradeRuleCondition([TradeRuleId], [CandleStickValueTypeId], [TradeRuleConditionComparatorId], [TradeRuleConditionSampleDirectionId], [FromMinutesOffset], [ToMinutesOffset], [FromMinutesSample], [ToMinutesSample], [DeltaPercent], [Description]) 
VALUES (1, 5, 1, 2, -5*60, -4*60, 30, 30, -1.2, 'A drop of over 1.2% over 1 hour 4 hours ago')
INSERT INTO TradeRuleCondition([TradeRuleId], [CandleStickValueTypeId], [TradeRuleConditionComparatorId], [TradeRuleConditionSampleDirectionId], [FromMinutesOffset], [ToMinutesOffset], [FromMinutesSample], [ToMinutesSample], [DeltaPercent], [Description]) 
VALUES (1, 5, 3, 1, -4*60, -30, 60, 60, 0.5, 'No more then 0.5% change over the last 4 hours')
INSERT INTO TradeRuleCondition([TradeRuleId], [CandleStickValueTypeId], [TradeRuleConditionComparatorId], [TradeRuleConditionSampleDirectionId], [FromMinutesOffset], [ToMinutesOffset], [FromMinutesSample], [ToMinutesSample], [DeltaPercent], [Description]) 
VALUES (1, 5, 2, 5, -10*24*60, -12*60, 24*60, 24*60, 1.0, 'The general trend over 10 days is positive')