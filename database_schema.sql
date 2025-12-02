-- Database Schema

-- Orders
CREATE TABLE IF NOT EXISTS "Orders" (
    "Id" UUID PRIMARY KEY,
    "CustomerName" VARCHAR(100) NOT NULL,
    "ShippingAddress" VARCHAR(200) NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Status" INTEGER NOT NULL,
    "TotalAmount" DECIMAL(18, 2) NOT NULL
);

-- OrderItems
CREATE TABLE IF NOT EXISTS "OrderItems" (
    "Id" UUID PRIMARY KEY,
    "ProductId" UUID NOT NULL,
    "ProductName" VARCHAR(100) NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "UnitPrice" DECIMAL(18, 2) NOT NULL,
    "OrderId" UUID NOT NULL,
    CONSTRAINT "FK_OrderItems_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");

-- InventoryService
CREATE TABLE IF NOT EXISTS "InventoryItems" (
    "Id" UUID PRIMARY KEY,
    "ProductId" UUID NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "ReservedQuantity" INTEGER NOT NULL
);

CREATE INDEX IF NOT EXISTS "IX_InventoryItems_ProductId" ON "InventoryItems" ("ProductId");
