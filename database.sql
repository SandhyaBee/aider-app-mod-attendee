-- 1. SETUP STRUCTURE
DROP TABLE IF EXISTS OrderItems; DROP TABLE IF EXISTS Orders;
DROP TABLE IF EXISTS ShoppingCartItems; DROP TABLE IF EXISTS ProductTags;
DROP TABLE IF EXISTS Tags; DROP TABLE IF EXISTS Products; DROP TABLE IF EXISTS Categories;

CREATE TABLE Categories (Id INT PRIMARY KEY IDENTITY(1,1), Name NVARCHAR(100) NOT NULL);
CREATE TABLE Tags (Id INT PRIMARY KEY IDENTITY(1,1), Name NVARCHAR(50) NOT NULL);

CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    Price DECIMAL(10, 2) NOT NULL,
    CategoryId INT FOREIGN KEY REFERENCES Categories(Id),
    InventoryCount INT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE ProductTags (
    ProductId INT FOREIGN KEY REFERENCES Products(Id),
    TagId INT FOREIGN KEY REFERENCES Tags(Id),
    PRIMARY KEY (ProductId, TagId)
);

CREATE TABLE ShoppingCartItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SessionId NVARCHAR(100) NOT NULL, 
    ProductId INT FOREIGN KEY REFERENCES Products(Id),
    Quantity INT DEFAULT 1,
    DateAdded DATETIME DEFAULT GETDATE()
);

CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    TotalAmount DECIMAL(10, 2) NOT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    ShippingRegion NVARCHAR(50) NOT NULL, -- Values: 'East US', 'North Europe', 'East Asia'
    Status NVARCHAR(20) DEFAULT 'Pending'
);

-- 2. ORDER ITEMS TABLE (Many-to-Many Join)
-- Connects Orders to Products with snapshot pricing
CREATE TABLE OrderItems (
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10, 2) NOT NULL, -- Captured at time of purchase
    PRIMARY KEY (OrderId, ProductId),
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- 2. SEED CATEGORIES
SET IDENTITY_INSERT Categories ON;
INSERT INTO Categories (Id, Name) VALUES (1, 'Streetwear'), (2, 'Formal'), (3, 'Accessories'), (4, 'Techwear'), (5, 'Footwear');
SET IDENTITY_INSERT Categories OFF;

-- 3. SEED TAGS
SET IDENTITY_INSERT Tags ON;
INSERT INTO Tags (Id, Name) VALUES (1, 'Cotton'), (2, 'Unisex'), (3, 'Limited Edition'), (4, 'Summer'), (5, 'Waterproof'), (6, 'Recycled'), (7, 'Oversized'), (8, 'Neon'), (9, 'Breathable'), (10, 'Windproof');
SET IDENTITY_INSERT Tags OFF;

-- 4. SEED PRODUCTS (45 UNIQUE ITEMS)
SET IDENTITY_INSERT Products ON;
-- Streetwear (Category 1)
INSERT INTO Products (Id, Name, Description, Price, CategoryId, InventoryCount) VALUES 
(1, 'Galaxy Hoodie', '100% Cotton, heavy-weight oversized fit.', 59.99, 1, 150),
(2, 'Neon Joggers', 'Reflective street styling for night city life.', 75.00, 1, 80),
(3, 'Retro Logo Tee', 'Vintage wash with 80s inspired graphics.', 35.00, 1, 200),
(4, 'Acid Wash Denim', 'Distressed classic fit denim jacket.', 110.00, 1, 40),
(5, 'Skater Cargoes', 'Wide-leg utility pants with 8 pockets.', 85.50, 1, 120),
(6, 'Graffiti Windbreaker', 'Lightweight shell with artist collab print.', 95.00, 1, 60),
(7, 'Boxy Flannel', 'Brushed wool blend for layering.', 65.00, 1, 90),
(8, 'Street Beanie', 'Soft ribbed acrylic in various colors.', 25.00, 1, 300),
(9, 'Vanguard Vest', 'Utility tactical vest for urban layering.', 120.00, 1, 30);

-- Formal (Category 2)
INSERT INTO Products (Id, Name, Description, Price, CategoryId, InventoryCount) VALUES 
(10, 'Midnight Silk Dress', 'Elegance redefined in pure silk.', 250.00, 2, 25),
(11, 'Velvet Tuxedo Jacket', 'Deep emerald green velvet with satin lapels.', 320.00, 2, 15),
(12, 'Oxford Button-Down', 'Egyptian cotton slim-fit formal shirt.', 85.00, 2, 100),
(13, 'Tapered Wool Slacks', 'Italian wool blend, tailored for comfort.', 140.00, 2, 50),
(14, 'Satin Evening Gown', 'Floor length with a dramatic slit.', 400.00, 2, 10),
(15, 'Charcoal Three-Piece', 'The ultimate professional suit set.', 550.00, 2, 20),
(16, 'Silk Pocket Square', 'Hand-rolled edges with geometric patterns.', 30.00, 2, 200),
(17, 'Lace Cocktail Dress', 'Intricate floral lace over nude lining.', 180.00, 2, 35),
(18, 'Cufflink Set - Silver', 'Engraved minimalist architectural design.', 55.00, 2, 150);

-- Accessories (Category 3)
INSERT INTO Products (Id, Name, Description, Price, CategoryId, InventoryCount) VALUES 
(19, 'Urban Snapback', 'Structured 6-panel with 3D embroidery.', 28.00, 3, 500),
(20, 'Cyber Visor', 'LED-integrated eyewear for the bold.', 150.00, 3, 40),
(21, 'Leather Utility Belt', 'Modular pouch system for urban travel.', 95.00, 3, 75),
(22, 'Aviator Shades', 'Classic gold frames with polarized tint.', 120.00, 3, 60),
(23, 'Smart Fabric Scarf', 'Changes color based on temperature.', 85.00, 3, 110),
(24, 'Canvas Tote - StyleVerse', 'Reinforced eco-friendly daily shopper.', 20.00, 3, 1000),
(25, 'Enamel Pin Set', 'Custom icons from the StyleVerse lore.', 15.00, 3, 400),
(26, 'Titanium Chain', 'Industrial grade chunky neckwear.', 130.00, 3, 25),
(27, 'Tech Glove Liners', 'Touchscreen compatible thin liners.', 35.00, 3, 200);

-- Techwear (Category 4)
INSERT INTO Products (Id, Name, Description, Price, CategoryId, InventoryCount) VALUES 
(28, 'Apex Hardshell', 'Triple-layer Gore-Tex compatible shell.', 450.00, 4, 15),
(29, 'Modular Poncho', 'Convertible silhouette for extreme weather.', 210.00, 4, 30),
(30, 'Paratrooper Pants', 'Reinforced knees and quick-release straps.', 195.00, 4, 45),
(31, 'Stealth Mask', 'Activated carbon filter with mesh exterior.', 45.00, 4, 250),
(32, 'Thermal Base Layer', 'Moisture-wicking compression tech.', 70.00, 4, 150),
(33, 'Signal Blocker Pouch', 'RFID shielding for mobile devices.', 40.00, 4, 300),
(34, 'Exo-Sling Bag', 'Ergonomic cross-body with tablet sleeve.', 140.00, 4, 80),
(35, 'Liquid Metal Jersey', 'High-shine synthetic with UV protection.', 90.00, 4, 100),
(36, 'Ventilated Vest', 'Laser-cut perforations for max airflow.', 110.00, 4, 55);

-- Footwear (Category 5)
INSERT INTO Products (Id, Name, Description, Price, CategoryId, InventoryCount) VALUES 
(37, 'Nebula Runners', 'Glow-in-the-dark soles, mesh upper.', 160.00, 5, 120),
(38, 'Titan Combat Boots', 'Composite toe with waterproof zip.', 220.00, 5, 40),
(39, 'Lunar Sandals', 'Memory foam footbed with neoprene straps.', 65.00, 5, 200),
(40, 'Circuit High-Tops', 'Electronic integrated color-changing strips.', 300.00, 5, 20),
(41, 'Urban Suede Loafers', 'Premium leather interior, daily comfort.', 140.00, 5, 65),
(42, 'Track Spike Pro', 'Carbon fiber plate for explosive speed.', 250.00, 5, 30),
(43, 'Cloud Sliders', 'Single-mold EVA foam for post-gym.', 45.00, 5, 400),
(44, 'Vintage Court Shoes', 'Leather heritage design, gum sole.', 95.00, 5, 150),
(45, 'Cyber Sock-Shoe', 'Knitted upper with rugged rubber outsole.', 135.00, 5, 90);
SET IDENTITY_INSERT Products OFF;

-- 5. MAP TAGS TO PRODUCTS (Many-to-Many)
INSERT INTO ProductTags (ProductId, TagId) VALUES 
(1,1), (1,2), (1,7), -- Galaxy Hoodie: Cotton, Unisex, Oversized
(2,8), (2,5),       -- Neon Joggers: Neon, Waterproof
(10,3), (11,3),      -- Formal Items: Limited Edition
(28,5), (28,10), (28,9), -- Apex Shell: Waterproof, Windproof, Breathable
(37,8), (37,9),      -- Nebula Runners: Neon, Breathable
(40,8), (40,3);      -- Circuit High-Tops: Neon, Limited Edition
-- (Add more mappings as needed for the frontend filters)

-- 6. STARTING CARTS
INSERT INTO ShoppingCartItems (SessionId, ProductId, Quantity) VALUES 
('session-101', 1, 1), ('session-101', 37, 1), 
('session-102', 28, 1);