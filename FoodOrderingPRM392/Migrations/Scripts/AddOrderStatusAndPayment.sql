-- =====================================================
-- Migration: AddOrderStatusAndPayment
-- Description: Add Order Status, Payment fields, and Store enhancements
-- Run this script manually on your database if EF migrations fail
-- =====================================================

-- 1. Add new columns to Orders table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'Status')
BEGIN
    ALTER TABLE Orders ADD [Status] int NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'PaymentMethod')
BEGIN
    ALTER TABLE Orders ADD [PaymentMethod] int NOT NULL DEFAULT 1;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'PaymentStatus')
BEGIN
    ALTER TABLE Orders ADD [PaymentStatus] int NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'SubTotal')
BEGIN
    ALTER TABLE Orders ADD [SubTotal] money NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'DeliveryFee')
BEGIN
    ALTER TABLE Orders ADD [DeliveryFee] money NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'Discount')
BEGIN
    ALTER TABLE Orders ADD [Discount] money NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'StoreId')
BEGIN
    ALTER TABLE Orders ADD [StoreId] bigint NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'DeliveryAddress')
BEGIN
    ALTER TABLE Orders ADD [DeliveryAddress] nvarchar(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'RecipientPhone')
BEGIN
    ALTER TABLE Orders ADD [RecipientPhone] varchar(15) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'RecipientName')
BEGIN
    ALTER TABLE Orders ADD [RecipientName] nvarchar(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'Note')
BEGIN
    ALTER TABLE Orders ADD [Note] nvarchar(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'CancelReason')
BEGIN
    ALTER TABLE Orders ADD [CancelReason] nvarchar(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'ConfirmedAt')
BEGIN
    ALTER TABLE Orders ADD [ConfirmedAt] datetime2 NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'CompletedAt')
BEGIN
    ALTER TABLE Orders ADD [CompletedAt] datetime2 NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'CancelledAt')
BEGIN
    ALTER TABLE Orders ADD [CancelledAt] datetime2 NULL;
END
GO

-- 2. Add new columns to Stores table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Stores') AND name = 'Phone')
BEGIN
    ALTER TABLE Stores ADD [Phone] varchar(15) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Stores') AND name = 'IsOpen')
BEGIN
    ALTER TABLE Stores ADD [IsOpen] bit NOT NULL DEFAULT 1;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Stores') AND name = 'Rating')
BEGIN
    ALTER TABLE Stores ADD [Rating] decimal(3,2) NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Stores') AND name = 'ReviewCount')
BEGIN
    ALTER TABLE Stores ADD [ReviewCount] int NOT NULL DEFAULT 0;
END
GO

-- 3. Add Foreign Key: Orders -> Stores
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Orders_Stores_StoreId')
BEGIN
    -- First, update existing orders to have a valid StoreId (get from first OrderDetail)
    UPDATE o
    SET o.StoreId = (
        SELECT TOP 1 fs.StoreId 
        FROM OrderDetails od 
        JOIN FoodStores fs ON od.FoodStoreId = fs.Id 
        WHERE od.OrderId = o.Id
    )
    FROM Orders o
    WHERE o.StoreId IS NULL;

    -- Make StoreId NOT NULL after populating
    ALTER TABLE Orders ALTER COLUMN [StoreId] bigint NOT NULL;

    -- Add FK constraint
    ALTER TABLE Orders ADD CONSTRAINT [FK_Orders_Stores_StoreId] 
        FOREIGN KEY ([StoreId]) REFERENCES [Stores] ([Id]) ON DELETE NO ACTION;
END
GO

-- 4. Create Indexes for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_Status')
BEGIN
    CREATE INDEX [IX_Orders_Status] ON [Orders] ([Status]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_UserId_PurchaseDate')
BEGIN
    CREATE INDEX [IX_Orders_UserId_PurchaseDate] ON [Orders] ([UserId], [PurchaseDate]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_StoreId_Status')
BEGIN
    CREATE INDEX [IX_Orders_StoreId_Status] ON [Orders] ([StoreId], [Status]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_StoreId')
BEGIN
    CREATE INDEX [IX_Orders_StoreId] ON [Orders] ([StoreId]);
END
GO

-- 5. Update existing orders to have SubTotal = Total
UPDATE Orders SET SubTotal = Total WHERE SubTotal = 0 AND Total > 0;
GO

-- 6. Update existing orders PaymentStatus to Paid if they exist (legacy orders were always paid)
UPDATE Orders SET PaymentStatus = 1 WHERE PaymentStatus = 0;
GO

PRINT 'Migration AddOrderStatusAndPayment completed successfully!';
GO
