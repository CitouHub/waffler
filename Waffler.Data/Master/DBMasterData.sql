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

SET IDENTITY_INSERT TradeRuleStatus ON
GO
INSERT INTO TradeRuleStatus (ID, Name) VALUES (1, 'Inactive')
INSERT INTO TradeRuleStatus (ID, Name) VALUES (2, 'Test')
INSERT INTO TradeRuleStatus (ID, Name) VALUES (3, 'Active')
SET IDENTITY_INSERT TradeRuleStatus OFF
GO

SET IDENTITY_INSERT TradeRuleConditionSampleDirection ON
GO
INSERT INTO TradeRuleConditionSampleDirection (ID, Name, Description) VALUES (1, 'Centered', 'The FromDate is sampled left and right from the sample point')
INSERT INTO TradeRuleConditionSampleDirection (ID, Name, Description) VALUES (2, 'Left shift', 'The FromDate is sampled left from the sample point')
INSERT INTO TradeRuleConditionSampleDirection (ID, Name, Description) VALUES (3, 'Right shift', 'The FromDate is sampled right from the sample point')
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
INSERT INTO TradeRule([Id], [TradeActionId], [TradeTypeId], [TradeConditionOperatorId], [TradeRuleStatusId], [Name], [Amount], [TradeMinIntervalMinutes]) VALUES (1, 1, 1, 1, 2, 'Fast drop, small buy', 10, 60*2)
INSERT INTO TradeRule([Id], [TradeActionId], [TradeTypeId], [TradeConditionOperatorId], [TradeRuleStatusId], [Name], [Amount], [TradeMinIntervalMinutes]) VALUES (2, 1, 1, 1, 2, 'Slow drop, big buy', 40, 60*6)
INSERT INTO TradeRule([Id], [TradeActionId], [TradeTypeId], [TradeConditionOperatorId], [TradeRuleStatusId], [Name], [Amount], [TradeMinIntervalMinutes]) VALUES (3, 1, 1, 2, 2, 'Buy dayly', 20, 24*60)
SET IDENTITY_INSERT TradeRule OFF
GO

INSERT INTO TradeRuleCondition([TradeRuleId], [CandleStickValueTypeId], [TradeRuleConditionComparatorId], [TradeRuleConditionSampleDirectionId], [FromMinutesOffset], [ToMinutesOffset], [FromMinutesSample], [ToMinutesSample], [DeltaPercent], [Description], [IsOn]) 
VALUES (1, 6, 1, 2, -4*60, -1*60, 60, 60, -3, 'A drop of over 3% now', 1)

INSERT INTO TradeRuleCondition([TradeRuleId], [CandleStickValueTypeId], [TradeRuleConditionComparatorId], [TradeRuleConditionSampleDirectionId], [FromMinutesOffset], [ToMinutesOffset], [FromMinutesSample], [ToMinutesSample], [DeltaPercent], [Description], [IsOn]) 
VALUES (2, 6, 1, 1, -4*24*60, -1*24*60, 6*60, 6*60, -6, 'A drop of over 6% over 3 days 1 day ago', 1)

INSERT INTO TradeRuleCondition([TradeRuleId], [CandleStickValueTypeId], [TradeRuleConditionComparatorId], [TradeRuleConditionSampleDirectionId], [FromMinutesOffset], [ToMinutesOffset], [FromMinutesSample], [ToMinutesSample], [DeltaPercent], [Description], [IsOn]) 
VALUES (3, 6, 2, 1, -60, -60, 30, 30, 0.0, 'Any change', 1)