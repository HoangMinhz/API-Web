-- Insert Categories
INSERT INTO Categories (Name, ImageUrl) VALUES 
('Laptop', 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8'),
('Điện thoại', 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9'),
('Phụ kiện', 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8'),
('Màn hình', 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8'),
('Thiết bị mạng', 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8');

-- Insert Products
INSERT INTO Products (Name, Description, Price, Stock, ImageUrl, CategoryId, Rating, ReviewCount, SoldCount, CreatedAt) VALUES
-- Laptops
('Laptop Dell XPS 13', 'Laptop siêu mỏng nhẹ, màn hình 13.3 inch, chip Intel Core i7', 29990000, 50, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8', 1, 4.5, 120, 45, datetime('now')),
('MacBook Pro M2', 'Laptop Apple 14 inch, chip M2, 16GB RAM', 39990000, 30, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8', 1, 4.8, 95, 28, datetime('now')),
('Asus ROG Zephyrus', 'Laptop gaming, RTX 3070, 16GB RAM', 34990000, 25, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8', 1, 4.6, 80, 22, datetime('now')),

-- Smartphones
('iPhone 15 Pro Max', 'Điện thoại Apple, chip A17 Pro, camera 48MP', 34990000, 100, 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9', 2, 4.7, 150, 85, datetime('now')),
('Samsung Galaxy S24 Ultra', 'Điện thoại Android, S Pen, camera 200MP', 29990000, 80, 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9', 2, 4.6, 130, 75, datetime('now')),
('Xiaomi 14 Pro', 'Điện thoại Android, chip Snapdragon 8 Gen 3', 19990000, 60, 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9', 2, 4.5, 90, 55, datetime('now')),

-- Accessories
('Tai nghe AirPods Pro 2', 'Tai nghe không dây Apple, chống ồn chủ động', 6990000, 200, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8', 3, 4.8, 200, 180, datetime('now')),
('Chuột Logitech MX Master 3S', 'Chuột không dây cao cấp, cảm biến 8000 DPI', 2990000, 150, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8', 3, 4.7, 160, 140, datetime('now')),
('Bàn phím cơ Keychron K8', 'Bàn phím cơ không dây, switch Gateron', 2490000, 100, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8', 3, 4.6, 120, 95, datetime('now')),

-- Monitors
('Màn hình Dell UltraSharp 27', 'Màn hình 27 inch 4K, 99% sRGB', 12990000, 40, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8', 4, 4.7, 85, 35, datetime('now')),
('LG UltraGear 32GP850', 'Màn hình gaming 32 inch, 165Hz, QHD', 14990000, 35, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8', 4, 4.6, 75, 30, datetime('now')),
('Samsung Odyssey G7', 'Màn hình cong 27 inch, 240Hz, QHD', 16990000, 30, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8', 4, 4.8, 95, 28, datetime('now')),

-- Networking
('Router Asus RT-AX86U', 'Router WiFi 6, tốc độ 5700Mbps', 4990000, 50, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8', 5, 4.5, 60, 45, datetime('now')),
('Switch Cisco SG350', 'Switch 24 port Gigabit, quản lý được', 8990000, 25, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8', 5, 4.7, 45, 22, datetime('now')),
('Access Point Ubiquiti U6-Pro', 'Access Point WiFi 6, phủ sóng rộng', 3990000, 40, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8', 5, 4.6, 50, 35, datetime('now'));

-- Insert User Roles
INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES
(1, 'Admin', 'ADMIN', 'ROLE_CONCURRENCY_STAMP_1'),
(2, 'Customer', 'CUSTOMER', 'ROLE_CONCURRENCY_STAMP_2');

-- Insert Users (using IdentityUser structure with integer keys)
INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FirstName, LastName, Address) VALUES
(1, 'admin@techstore.vn', 'ADMIN@TECHSTORE.VN', 'admin@techstore.vn', 'ADMIN@TECHSTORE.VN', 1, 'AQAAAAEAACcQAAAAEHYxVHXJQJQJQJQJQJQJQJQJQJQJQJQJQJQJQJQJQ==', 'SECURITY_STAMP_1', 'CONCURRENCY_STAMP_1', NULL, 0, 0, NULL, 1, 0, 'Admin', 'User', '123 Admin Street'),
(2, 'customer1@email.com', 'CUSTOMER1@EMAIL.COM', 'customer1@email.com', 'CUSTOMER1@EMAIL.COM', 1, 'AQAAAAEAACcQAAAAEHYxVHXJQJQJQJQJQJQJQJQJQJQJQJQJQJQJQJQJQJQJQ==', 'SECURITY_STAMP_2', 'CONCURRENCY_STAMP_2', NULL, 0, 0, NULL, 1, 0, 'Customer', 'One', '456 Customer Street'),
(3, 'customer2@email.com', 'CUSTOMER2@EMAIL.COM', 'customer2@email.com', 'CUSTOMER2@EMAIL.COM', 1, 'AQAAAAEAACcQAAAAEHYxVHXJQJQJQJQJQJQJQJQJQJQJQJQJQJQJQJQJQJQJQ==', 'SECURITY_STAMP_3', 'CONCURRENCY_STAMP_3', NULL, 0, 0, NULL, 1, 0, 'Customer', 'Two', '789 Customer Street');

-- Assign Roles to Users
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES
(1, 1),  -- Admin user is Admin
(2, 2),  -- Customer1 is Customer
(3, 2);  -- Customer2 is Customer 

-- Update Category Images
UPDATE Categories SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 1; -- Laptop
UPDATE Categories SET ImageUrl = 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9' WHERE Id = 2; -- Phone
UPDATE Categories SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 3; -- Accessories
UPDATE Categories SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 4; -- Monitor
UPDATE Categories SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 5; -- Network

-- Update Product Images
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 1; -- Dell XPS 13
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 2; -- MacBook Pro M2
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 3; -- Asus ROG
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9' WHERE Id = 4; -- iPhone 15 Pro
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9' WHERE Id = 5; -- Samsung S24
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9' WHERE Id = 6; -- Xiaomi 14
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 7; -- AirPods Pro
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 8; -- Logitech MX
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 9; -- Keychron K8
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 10; -- Dell UltraSharp
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 11; -- LG UltraGear
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 12; -- Samsung Odyssey
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 13; -- Asus Router
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 14; -- Cisco Switch
UPDATE Products SET ImageUrl = 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8' WHERE Id = 15; -- Ubiquiti AP 