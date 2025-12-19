-- wms-init.sql
CREATE DATABASE erp_demo;

\c erp_demo;

DROP TABLE IF EXISTS stock_inventory;
DROP TABLE IF EXISTS base_products;

CREATE TABLE base_products (
                               product_id SERIAL PRIMARY KEY,
                               product_name VARCHAR(200) NOT NULL,
                               sku_code VARCHAR(50) NOT NULL,
                               category VARCHAR(50),
                               created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

COMMENT ON TABLE base_products IS '基础商品信息表，包含产品ID、名称和SKU编码';
COMMENT ON COLUMN base_products.product_id IS '唯一商品ID';
COMMENT ON COLUMN base_products.product_name IS '商品名称，如 iPhone 15 Pro';
COMMENT ON COLUMN base_products.sku_code IS '库存单元编码';

CREATE TABLE stock_inventory (
                                     id SERIAL PRIMARY KEY,
                                     product_id INT NOT NULL,
                                     warehouse_id INT NOT NULL,
                                     quantity INT DEFAULT 0,
                                     last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                     CONSTRAINT fk_product FOREIGN KEY (product_id) REFERENCES base_products(product_id)
);

COMMENT ON TABLE stock_inventory IS '实时库存表，记录各仓库中商品的数量';
COMMENT ON COLUMN stock_inventory.product_id IS '关联商品ID';
COMMENT ON COLUMN stock_inventory.warehouse_id IS '仓库ID (1=杭州仓, 2=北京仓)';
COMMENT ON COLUMN stock_inventory.quantity IS '当前可用库存数量';

INSERT INTO base_products (product_name, sku_code, category) VALUES
                                                                 ('iPhone 15 Pro 256G', 'AP-IP15P-256-BLK', 'Electronics'),
                                                                 ('iPhone 15 Plus 128G', 'AP-IP15PL-128-WHT', 'Electronics'),
                                                                 ('MacBook Pro M3', 'AP-MBP-M3-14', 'Electronics'),
                                                                 ('Dell XPS 15', 'DELL-XPS-9530', 'Electronics'),
                                                                 ('Logitech MX Master 3S', 'LOGI-MX3S', 'Accessories');

INSERT INTO stock_inventory (product_id, warehouse_id, quantity) VALUES
                                                                         (1, 1, 30), (1, 2, 12), (2, 1, 18), (3, 1, 5), (4, 2, 8), (5, 1, 100);