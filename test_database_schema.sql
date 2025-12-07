-- =============================================================================
-- TargCC V2 - Test Database Schema
-- Purpose: Sample database for testing TargCC V2 code generation
-- Tables: 4 (Customer, Order, Product, OrderItem)
-- Features: Prefixes (ent_, lkp_, enm_, clc_), Foreign Keys, Indexes
-- =============================================================================

-- Create database (if needed)
-- CREATE DATABASE TargCCTest;
-- GO

-- USE TargCCTest;
-- GO

-- =============================================================================
-- Table 1: Customer
-- Demonstrates: ent_ (encrypted), lkp_ (lookup), audit fields, unique index
-- =============================================================================

PRINT 'Creating table: Customer';
GO

CREATE TABLE [dbo].[Customer] (
    -- Primary Key
    [ID] INT PRIMARY KEY IDENTITY(1,1),

    -- Basic fields
    [Name] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(100) NOT NULL,
    [Phone] VARCHAR(20),

    -- Special Prefixes
    [ent_CreditCard] NVARCHAR(MAX),     -- ent_ = Encrypted (two-way)
    [lkp_Status] VARCHAR(20),            -- lkp_ = Lookup table reference

    -- Audit fields (TargCC recognizes these)
    [AddedOn] DATETIME NOT NULL DEFAULT GETDATE(),
    [AddedBy] NVARCHAR(50),
    [ChangedOn] DATETIME,
    [ChangedBy] NVARCHAR(50)
);
GO

-- Indexes
CREATE UNIQUE INDEX IX_Customer_Email ON [dbo].[Customer]([Email]);
CREATE INDEX IX_Customer_Status ON [dbo].[Customer]([lkp_Status]);
GO

PRINT '  ✓ Customer table created with 2 indexes';
GO

-- =============================================================================
-- Table 2: Order
-- Demonstrates: Foreign Key, enm_ (enum), relationship
-- =============================================================================

PRINT 'Creating table: Order';
GO

CREATE TABLE [dbo].[Order] (
    -- Primary Key
    [ID] INT PRIMARY KEY IDENTITY(1,1),

    -- Foreign Key
    [CustomerID] INT NOT NULL,

    -- Basic fields
    [OrderDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [TotalAmount] DECIMAL(18,2),

    -- Special Prefix
    [enm_Status] VARCHAR(20),            -- enm_ = Enum (generates C# enum)

    -- Audit
    [AddedOn] DATETIME NOT NULL DEFAULT GETDATE(),

    -- Foreign Key Constraint
    CONSTRAINT FK_Order_Customer FOREIGN KEY ([CustomerID])
        REFERENCES [dbo].[Customer]([ID])
        ON DELETE CASCADE
);
GO

-- Indexes
CREATE INDEX IX_Order_Customer ON [dbo].[Order]([CustomerID]);
CREATE INDEX IX_Order_Date ON [dbo].[Order]([OrderDate]);
CREATE INDEX IX_Order_Status ON [dbo].[Order]([enm_Status]);
GO

PRINT '  ✓ Order table created with 3 indexes and 1 FK';
GO

-- =============================================================================
-- Table 3: Product
-- Demonstrates: enm_ (enum), bit field, multiple indexes
-- =============================================================================

PRINT 'Creating table: Product';
GO

CREATE TABLE [dbo].[Product] (
    -- Primary Key
    [ID] INT PRIMARY KEY IDENTITY(1,1),

    -- Basic fields
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX),
    [Price] DECIMAL(18,2) NOT NULL,
    [StockQuantity] INT NOT NULL,

    -- Special Prefix
    [enm_Category] VARCHAR(50),          -- enm_ = Enum

    -- Flags
    [IsActive] BIT NOT NULL DEFAULT 1,

    -- Audit
    [AddedOn] DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Indexes
CREATE INDEX IX_Product_Name ON [dbo].[Product]([Name]);
CREATE INDEX IX_Product_Category ON [dbo].[Product]([enm_Category]);
CREATE INDEX IX_Product_Active ON [dbo].[Product]([IsActive]) WHERE [IsActive] = 1;
GO

PRINT '  ✓ Product table created with 3 indexes';
GO

-- =============================================================================
-- Table 4: OrderItem (Junction Table)
-- Demonstrates: Many-to-Many, clc_ (calculated), composite FK
-- =============================================================================

PRINT 'Creating table: OrderItem';
GO

CREATE TABLE [dbo].[OrderItem] (
    -- Primary Key
    [ID] INT PRIMARY KEY IDENTITY(1,1),

    -- Foreign Keys
    [OrderID] INT NOT NULL,
    [ProductID] INT NOT NULL,

    -- Quantity and Price
    [Quantity] INT NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,

    -- Special Prefix: Calculated Field
    [clc_LineTotal] AS ([Quantity] * [UnitPrice]),  -- clc_ = Calculated (read-only)

    -- Foreign Key Constraints
    CONSTRAINT FK_OrderItem_Order FOREIGN KEY ([OrderID])
        REFERENCES [dbo].[Order]([ID])
        ON DELETE CASCADE,

    CONSTRAINT FK_OrderItem_Product FOREIGN KEY ([ProductID])
        REFERENCES [dbo].[Product]([ID])
        ON DELETE CASCADE
);
GO

-- Indexes
CREATE INDEX IX_OrderItem_Order ON [dbo].[OrderItem]([OrderID]);
CREATE INDEX IX_OrderItem_Product ON [dbo].[OrderItem]([ProductID]);
GO

PRINT '  ✓ OrderItem table created with 2 indexes and 2 FKs';
GO

-- =============================================================================
-- VIEWS (Read-Only Reporting)
-- Demonstrates: Complex queries, aggregations, joins - will generate read-only UI
-- =============================================================================

PRINT 'Creating views for reporting...';
GO

-- View 1: Customer Order Summary
CREATE VIEW [dbo].[vw_CustomerOrderSummary] AS
SELECT
    c.ID AS CustomerID,
    c.Name AS CustomerName,
    c.Email,
    c.lkp_Status AS Status,
    COUNT(o.ID) AS TotalOrders,
    SUM(o.TotalAmount) AS TotalSpent,
    MAX(o.OrderDate) AS LastOrderDate,
    MIN(o.OrderDate) AS FirstOrderDate
FROM
    [dbo].[Customer] c
LEFT JOIN
    [dbo].[Order] o ON c.ID = o.CustomerID
GROUP BY
    c.ID, c.Name, c.Email, c.lkp_Status;
GO

PRINT '  ✓ vw_CustomerOrderSummary created (aggregated customer data)';
GO

-- View 2: Order Details with Customer Info
CREATE VIEW [dbo].[vw_OrderDetails] AS
SELECT
    o.ID AS OrderID,
    o.OrderDate,
    o.TotalAmount,
    o.enm_Status AS OrderStatus,
    c.ID AS CustomerID,
    c.Name AS CustomerName,
    c.Email AS CustomerEmail,
    c.Phone AS CustomerPhone,
    COUNT(oi.ID) AS ItemCount,
    SUM(oi.Quantity) AS TotalQuantity
FROM
    [dbo].[Order] o
INNER JOIN
    [dbo].[Customer] c ON o.CustomerID = c.ID
LEFT JOIN
    [dbo].[OrderItem] oi ON o.ID = oi.OrderID
GROUP BY
    o.ID, o.OrderDate, o.TotalAmount, o.enm_Status,
    c.ID, c.Name, c.Email, c.Phone;
GO

PRINT '  ✓ vw_OrderDetails created (orders with customer info)';
GO

-- View 3: Product Sales Report
CREATE VIEW [dbo].[vw_ProductSales] AS
SELECT
    p.ID AS ProductID,
    p.Name AS ProductName,
    p.enm_Category AS Category,
    p.Price AS CurrentPrice,
    p.StockQuantity,
    p.IsActive,
    COUNT(oi.ID) AS TimesSold,
    SUM(oi.Quantity) AS TotalQuantitySold,
    SUM(oi.clc_LineTotal) AS TotalRevenue,
    AVG(oi.UnitPrice) AS AverageSellingPrice
FROM
    [dbo].[Product] p
LEFT JOIN
    [dbo].[OrderItem] oi ON p.ID = oi.ProductID
GROUP BY
    p.ID, p.Name, p.enm_Category, p.Price, p.StockQuantity, p.IsActive;
GO

PRINT '  ✓ vw_ProductSales created (product performance analytics)';
GO

-- =============================================================================
-- Sample Data
-- =============================================================================

PRINT 'Inserting sample data...';
GO

-- Customers
INSERT INTO [dbo].[Customer] ([Name], [Email], [Phone], [lkp_Status], [AddedBy])
VALUES
    ('John Doe', 'john.doe@example.com', '555-0100', 'Active', 'system'),
    ('Jane Smith', 'jane.smith@example.com', '555-0200', 'Active', 'system'),
    ('Bob Johnson', 'bob.johnson@example.com', '555-0300', 'Inactive', 'system'),
    ('Alice Williams', 'alice.williams@example.com', '555-0400', 'Active', 'system'),
    ('Charlie Brown', 'charlie.brown@example.com', '555-0500', 'Pending', 'system');
GO

PRINT '  ✓ Inserted 5 customers';
GO

-- Products
INSERT INTO [dbo].[Product] ([Name], [Description], [Price], [StockQuantity], [enm_Category], [IsActive])
VALUES
    ('Laptop Dell XPS 13', 'High-performance ultrabook', 1299.99, 15, 'Electronics', 1),
    ('iPhone 15 Pro', 'Latest Apple smartphone', 999.99, 30, 'Electronics', 1),
    ('Office Chair', 'Ergonomic mesh office chair', 249.99, 50, 'Furniture', 1),
    ('Standing Desk', 'Adjustable height desk', 599.99, 20, 'Furniture', 1),
    ('Wireless Mouse', 'Logitech MX Master 3', 99.99, 100, 'Accessories', 1),
    ('Mechanical Keyboard', 'Cherry MX switches', 149.99, 75, 'Accessories', 1);
GO

PRINT '  ✓ Inserted 6 products';
GO

-- Orders
INSERT INTO [dbo].[Order] ([CustomerID], [OrderDate], [TotalAmount], [enm_Status])
VALUES
    (1, '2025-12-01', 1599.98, 'Completed'),
    (1, '2025-12-03', 249.99, 'Shipped'),
    (2, '2025-12-02', 999.99, 'Processing'),
    (3, '2025-11-28', 849.97, 'Completed'),
    (4, '2025-12-04', 1449.98, 'Pending');
GO

PRINT '  ✓ Inserted 5 orders';
GO

-- Order Items
INSERT INTO [dbo].[OrderItem] ([OrderID], [ProductID], [Quantity], [UnitPrice])
VALUES
    -- Order 1 (John)
    (1, 1, 1, 1299.99),  -- Laptop
    (1, 5, 3, 99.99),    -- Mouse x3

    -- Order 2 (John)
    (2, 3, 1, 249.99),   -- Chair

    -- Order 3 (Jane)
    (3, 2, 1, 999.99),   -- iPhone

    -- Order 4 (Bob)
    (4, 4, 1, 599.99),   -- Desk
    (4, 5, 1, 99.99),    -- Mouse
    (4, 6, 1, 149.99),   -- Keyboard

    -- Order 5 (Alice)
    (5, 1, 1, 1299.99),  -- Laptop
    (5, 6, 1, 149.99);   -- Keyboard
GO

PRINT '  ✓ Inserted 9 order items';
GO

-- =============================================================================
-- Verification Queries
-- =============================================================================

PRINT '';
PRINT '========================================';
PRINT 'Database Created Successfully!';
PRINT '========================================';
PRINT '';

-- Show table counts
PRINT 'Table Statistics:';
SELECT
    t.name AS TableName,
    p.rows AS [RowCount]
FROM
    sys.tables t
INNER JOIN
    sys.partitions p ON t.object_id = p.object_id
WHERE
    t.schema_id = SCHEMA_ID('dbo')
    AND p.index_id IN (0,1)
    AND t.name IN ('Customer', 'Order', 'Product', 'OrderItem')
ORDER BY
    t.name;
GO

-- Show foreign keys
PRINT '';
PRINT 'Foreign Key Relationships:';
SELECT
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS ChildTable,
    OBJECT_NAME(fk.referenced_object_id) AS ParentTable
FROM
    sys.foreign_keys fk
WHERE
    OBJECT_NAME(fk.parent_object_id) IN ('Order', 'OrderItem')
ORDER BY
    ChildTable, ForeignKeyName;
GO

PRINT '';
PRINT 'Ready for TargCC code generation!';
PRINT '';
PRINT 'Next steps:';
PRINT '  1. cd /your/project/directory';
PRINT '  2. targcc init';
PRINT '  3. targcc config set ConnectionString "Server=localhost;Database=TargCCTest;..."';
PRINT '  4. targcc generate project --database TargCCTest --output . --namespace MyApp';
PRINT '';
GO
