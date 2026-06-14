CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Login" VARCHAR(100) NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "Role" VARCHAR(50) NOT NULL
);

CREATE TABLE IF NOT EXISTS "Products" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Description" TEXT NOT NULL DEFAULT '',
    "QuantityInStock" INT NOT NULL DEFAULT 0,
    CONSTRAINT "CK_Products_NonNegativeStock" CHECK ("QuantityInStock" >= 0)
);

CREATE TABLE IF NOT EXISTS "Orders" (
    "Id" SERIAL PRIMARY KEY,
    "OrderNumber" VARCHAR(100) NOT NULL UNIQUE,
    "Status" VARCHAR(50) NOT NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "RecipientDocument" VARCHAR(200)
);

CREATE TABLE IF NOT EXISTS "OrderItems" (
    "Id" SERIAL PRIMARY KEY,
    "OrderId" INT NOT NULL REFERENCES "Orders"("Id") ON DELETE CASCADE,
    "ProductId" INT NOT NULL REFERENCES "Products"("Id") ON DELETE RESTRICT,
    "Quantity" INT NOT NULL CHECK ("Quantity" > 0)
);

CREATE TABLE IF NOT EXISTS "StockOperations" (
    "Id" SERIAL PRIMARY KEY,
    "ProductId" INT NOT NULL REFERENCES "Products"("Id") ON DELETE RESTRICT,
    "OperationType" VARCHAR(50) NOT NULL,
    "Quantity" INT NOT NULL CHECK ("Quantity" > 0),
    "Date" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "Comment" TEXT NOT NULL DEFAULT ''
);
