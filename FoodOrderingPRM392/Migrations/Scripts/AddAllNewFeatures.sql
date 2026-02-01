-- =====================================================
-- Migration: AddAllNewFeatures
-- Description: Add all new features: DeliveryAddress, Review, Voucher, Favorite, Notification
-- Run this script manually on your database
-- =====================================================

PRINT 'Starting migration: AddAllNewFeatures';
GO

-- ============ 1. DELIVERY ADDRESSES ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DeliveryAddresses')
BEGIN
    CREATE TABLE [DeliveryAddresses] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] bigint NOT NULL,
        [Label] nvarchar(50) NULL,
        [RecipientName] nvarchar(100) NULL,
        [Phone] varchar(15) NULL,
        [Address] nvarchar(500) NULL,
        [AddressDetail] nvarchar(200) NULL,
        [Latitude] varchar(20) NULL,
        [Longitude] varchar(20) NULL,
        [IsDefault] bit NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_DeliveryAddresses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DeliveryAddresses_Users] FOREIGN KEY ([UserId]) 
            REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_DeliveryAddresses_UserId_IsDefault] ON [DeliveryAddresses] ([UserId], [IsDefault]);
    PRINT 'Created table: DeliveryAddresses';
END
GO

-- ============ 2. REVIEWS ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews')
BEGIN
    CREATE TABLE [Reviews] (
        [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
        [UserId] bigint NOT NULL,
        [OrderId] uniqueidentifier NOT NULL,
        [StoreId] bigint NULL,
        [FoodId] bigint NULL,
        [Rating] int NOT NULL,
        [Comment] nvarchar(1000) NULL,
        [Images] nvarchar(max) NULL,
        [StoreReply] nvarchar(500) NULL,
        [StoreReplyAt] datetime2 NULL,
        [IsVisible] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Reviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reviews_Users] FOREIGN KEY ([UserId]) 
            REFERENCES [Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Reviews_Orders] FOREIGN KEY ([OrderId]) 
            REFERENCES [Orders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Reviews_Stores] FOREIGN KEY ([StoreId]) 
            REFERENCES [Stores] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Reviews_Foods] FOREIGN KEY ([FoodId]) 
            REFERENCES [Foods] ([Id]) ON DELETE NO ACTION
    );
    
    CREATE INDEX [IX_Reviews_StoreId_Rating] ON [Reviews] ([StoreId], [Rating]);
    CREATE INDEX [IX_Reviews_FoodId_Rating] ON [Reviews] ([FoodId], [Rating]);
    CREATE UNIQUE INDEX [IX_Reviews_OrderId] ON [Reviews] ([OrderId]);
    PRINT 'Created table: Reviews';
END
GO

-- ============ 3. VOUCHERS ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Vouchers')
BEGIN
    CREATE TABLE [Vouchers] (
        [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
        [Code] varchar(50) NOT NULL,
        [Name] nvarchar(200) NULL,
        [Description] nvarchar(500) NULL,
        [Type] int NOT NULL,
        [Value] money NOT NULL,
        [MinOrderAmount] money NOT NULL DEFAULT 0,
        [MaxDiscount] money NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [UsageLimit] int NULL,
        [UsageLimitPerUser] int NULL DEFAULT 1,
        [UsedCount] int NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT 1,
        [StoreId] bigint NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Vouchers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Vouchers_Stores] FOREIGN KEY ([StoreId]) 
            REFERENCES [Stores] ([Id]) ON DELETE CASCADE
    );
    
    CREATE UNIQUE INDEX [IX_Vouchers_Code] ON [Vouchers] ([Code]);
    CREATE INDEX [IX_Vouchers_IsActive_StartDate_EndDate] ON [Vouchers] ([IsActive], [StartDate], [EndDate]);
    PRINT 'Created table: Vouchers';
END
GO

-- ============ 4. VOUCHER USAGES ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'VoucherUsages')
BEGIN
    CREATE TABLE [VoucherUsages] (
        [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
        [VoucherId] uniqueidentifier NOT NULL,
        [UserId] bigint NOT NULL,
        [OrderId] uniqueidentifier NOT NULL,
        [DiscountAmount] money NOT NULL,
        [UsedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_VoucherUsages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VoucherUsages_Vouchers] FOREIGN KEY ([VoucherId]) 
            REFERENCES [Vouchers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_VoucherUsages_Users] FOREIGN KEY ([UserId]) 
            REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VoucherUsages_Orders] FOREIGN KEY ([OrderId]) 
            REFERENCES [Orders] ([Id]) ON DELETE NO ACTION
    );
    
    CREATE INDEX [IX_VoucherUsages_VoucherId_UserId] ON [VoucherUsages] ([VoucherId], [UserId]);
    CREATE UNIQUE INDEX [IX_VoucherUsages_OrderId] ON [VoucherUsages] ([OrderId]);
    PRINT 'Created table: VoucherUsages';
END
GO

-- ============ 5. FAVORITES ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Favorites')
BEGIN
    CREATE TABLE [Favorites] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] bigint NOT NULL,
        [StoreId] bigint NULL,
        [FoodId] bigint NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_Favorites] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Favorites_Users] FOREIGN KEY ([UserId]) 
            REFERENCES [Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Favorites_Stores] FOREIGN KEY ([StoreId]) 
            REFERENCES [Stores] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Favorites_Foods] FOREIGN KEY ([FoodId]) 
            REFERENCES [Foods] ([Id]) ON DELETE CASCADE
    );
    
    CREATE UNIQUE INDEX [IX_Favorites_UserId_StoreId] ON [Favorites] ([UserId], [StoreId]) WHERE [StoreId] IS NOT NULL;
    CREATE UNIQUE INDEX [IX_Favorites_UserId_FoodId] ON [Favorites] ([UserId], [FoodId]) WHERE [FoodId] IS NOT NULL;
    PRINT 'Created table: Favorites';
END
GO

-- ============ 6. NOTIFICATIONS ============
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
        [UserId] bigint NOT NULL,
        [Title] nvarchar(200) NULL,
        [Content] nvarchar(1000) NULL,
        [Type] int NOT NULL DEFAULT 0,
        [ReferenceId] varchar(100) NULL,
        [ImageUrl] varchar(500) NULL,
        [DeepLink] varchar(200) NULL,
        [IsRead] bit NOT NULL DEFAULT 0,
        [ReadAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Users] FOREIGN KEY ([UserId]) 
            REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_Notifications_UserId_IsRead_CreatedAt] ON [Notifications] ([UserId], [IsRead], [CreatedAt] DESC);
    PRINT 'Created table: Notifications';
END
GO

-- ============ 7. UPDATE USERS TABLE ============
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Users') AND name = 'AvatarUrl')
BEGIN
    ALTER TABLE Users ADD [AvatarUrl] varchar(500) NULL;
    PRINT 'Added column: Users.AvatarUrl';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Users') AND name = 'IsActive')
BEGIN
    ALTER TABLE Users ADD [IsActive] bit NOT NULL DEFAULT 1;
    PRINT 'Added column: Users.IsActive';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Users') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE Users ADD [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE();
    PRINT 'Added column: Users.CreatedAt';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Users') AND name = 'LastLoginAt')
BEGIN
    ALTER TABLE Users ADD [LastLoginAt] datetime2 NULL;
    PRINT 'Added column: Users.LastLoginAt';
END
GO

-- ============ 8. UPDATE STORES TABLE ============
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Stores') AND name = 'Description')
BEGIN
    ALTER TABLE Stores ADD [Description] nvarchar(1000) NULL;
    PRINT 'Added column: Stores.Description';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Stores') AND name = 'OpenTime')
BEGIN
    ALTER TABLE Stores ADD [OpenTime] varchar(10) NULL;
    PRINT 'Added column: Stores.OpenTime';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Stores') AND name = 'CloseTime')
BEGIN
    ALTER TABLE Stores ADD [CloseTime] varchar(10) NULL;
    PRINT 'Added column: Stores.CloseTime';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Stores') AND name = 'IsActive')
BEGIN
    ALTER TABLE Stores ADD [IsActive] bit NOT NULL DEFAULT 1;
    PRINT 'Added column: Stores.IsActive';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Stores') AND name = 'MinOrderAmount')
BEGIN
    ALTER TABLE Stores ADD [MinOrderAmount] money NOT NULL DEFAULT 0;
    PRINT 'Added column: Stores.MinOrderAmount';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Stores') AND name = 'DeliveryFee')
BEGIN
    ALTER TABLE Stores ADD [DeliveryFee] money NOT NULL DEFAULT 0;
    PRINT 'Added column: Stores.DeliveryFee';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Stores') AND name = 'EstimatedDeliveryTime')
BEGIN
    ALTER TABLE Stores ADD [EstimatedDeliveryTime] int NOT NULL DEFAULT 30;
    PRINT 'Added column: Stores.EstimatedDeliveryTime';
END
GO

-- ============ 9. UPDATE FOODS TABLE ============
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Foods') AND name = 'Description')
BEGIN
    ALTER TABLE Foods ADD [Description] nvarchar(1000) NULL;
    PRINT 'Added column: Foods.Description';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Foods') AND name = 'Rating')
BEGIN
    ALTER TABLE Foods ADD [Rating] decimal(3,2) NOT NULL DEFAULT 0;
    PRINT 'Added column: Foods.Rating';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Foods') AND name = 'ReviewCount')
BEGIN
    ALTER TABLE Foods ADD [ReviewCount] int NOT NULL DEFAULT 0;
    PRINT 'Added column: Foods.ReviewCount';
END
GO

-- ============ 10. UPDATE ORDERS TABLE (VoucherCode) ============
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Orders') AND name = 'VoucherCode')
BEGIN
    ALTER TABLE Orders ADD [VoucherCode] varchar(50) NULL;
    PRINT 'Added column: Orders.VoucherCode';
END
GO

PRINT '';
PRINT '=====================================================';
PRINT 'Migration AddAllNewFeatures completed successfully!';
PRINT '=====================================================';
GO
