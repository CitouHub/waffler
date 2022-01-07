-- The old TestTradeInProgress is no longer needed, but IsDeleted is, rename and reuse :)
EXEC sp_rename 'TradeRule.TestTradeInProgress', 'IsDeleted'
GO  