DELETE CandleStickValueType WHERE ID = 5

SET IDENTITY_INSERT CandleStickValueType ON
GO
INSERT INTO CandleStickValueType (ID, Name) VALUES (5, 'Volume')
SET IDENTITY_INSERT CandleStickValueType OFF
GO