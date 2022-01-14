UPDATE TradeRule SET Amount = CAST(Amount AS DECIMAL(10,2))

ALTER TABLE TradeRule ALTER COLUMN Amount [decimal](10,2)
GO