DROP TABLE IF EXISTS orders_import_lines;
CREATE TABLE orders_import_lines (
  id serial PRIMARY KEY,
  source_file text NOT NULL,   
  line_no int NOT NULL,        
  raw_line text NOT NULL,      
  imported_at timestamptz default now(),
  note text
);

INSERT INTO orders_import_lines (source_file, line_no, raw_line, note) VALUES
('marketplace_A_2025_11.csv', 1, 'Order#1001; Customer: Olga Petrova <olga.petrova@example.com>; +7 (921) 555-12-34; Items: SKU:AB-123-XY x1', 'order row'),
('marketplace_A_2025_11.csv', 2, 'Order#1002; Customer: Ivan <ivan@@example..com>; 8-921-5551234; Items: SKU:zx9999 x2', 'order row'),
('newsletter_upload.csv', 10, 'john.doe@domain.com; +44 7700 900123; tags: promo, holiday', 'marketing upload'),
('pricing_feed.csv', 3, 'product: ZX-11; price: "1,299.99" USD', 'price row'),
('pricing_feed.csv', 4, 'product: Y-200; price: "2 500,00" EUR', 'price row'),
('catalog_tags.csv', 1, 'tags: electronics, mobile,  accessories', 'tags row'),
('catalog_tags.csv', 2, 'tags: home,kitchen', 'tags row'),
('orders_dirty.csv', 5, '"Smith, John","12 Baker St, Apt 4","1,200.00","SKU: AB-123-XY"', 'dirty csv'),
('processor_log.txt', 100, 'INFO: Processing order 1001', 'log'),
('processor_log.txt', 101, 'warning: price parse failed for line 4', 'log'),
('processor_log.txt', 102, 'Error: invalid phone for order 1002', 'log'),
('processor_log.txt', 103, 'error: missing sku in items list', 'log'),
('marketplace_A_2025_11.csv', 20, 'Customer: bad@-domain.com; +7 921 ABC-12-34; Items: SKU: 12-AB-!!', 'trap-invalid-email-phone-sku'),
('orders_dirty.csv', 6, '"O\'Connor, Liam","New York, NY","500"', 'dirty csv with apostrophe');


SELECT *
FROM orders_import_lines
WHERE raw_line ~ '[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}';


SELECT *
FROM orders_import_lines
WHERE raw_line !~ '[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}';


SELECT 
    id,
    source_file,
    line_no,
    (regexp_match(raw_line, '[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}'))[1] AS email
FROM orders_import_lines
WHERE raw_line ~ '[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}';


SELECT 
    id,
    source_file,
    line_no,
    regexp_matches(raw_line, '[A-Z0-9]{2,}-[A-Z0-9-]+', 'gi') AS sku
FROM orders_import_lines
WHERE raw_line ~* '[A-Z0-9]{2,}-[A-Z0-9-]+';


SELECT 
    id,
    source_file,
    line_no,
    raw_line,
    regexp_replace(raw_line, '[^0-9]', '', 'g') AS normalized_phone
FROM orders_import_lines
WHERE raw_line ~ '[+0-9() -]{10,}';


SELECT 
    id,
    source_file,
    line_no,
    raw_line,
    CASE 
        WHEN raw_line ~ '"[\d, ]+\.\d{2}"' THEN 
            CAST(regexp_replace(
                regexp_replace(
                    (regexp_match(raw_line, '"([\d, ]+\.\d{2})"'))[1],
                    '[ ,]', 
                    '', 
                    'g'
                ),
                '\.',
                '.'
            ) AS NUMERIC)
        WHEN raw_line ~ '"[\d ]+,\d{2}"' THEN 
            CAST(regexp_replace(
                regexp_replace(
                    (regexp_match(raw_line, '"([\d ]+,\d{2})"'))[1],
                    '[ ]', 
                    '', 
                    'g'
                ),
                ',',
                '.'
            ) AS NUMERIC)
    END AS normalized_price
FROM orders_import_lines
WHERE raw_line ~ 'price:|Fare:|charge:|amount:';


SELECT 
    id,
    source_file,
    line_no,
    regexp_split_to_array(
        regexp_replace(
            regexp_replace(raw_line, '^.*tags:\s*', ''),
            '"', 
            ''
        ),
        '\s*,\s*'
    ) AS tags_array
FROM orders_import_lines
WHERE raw_line LIKE '%tags:%';


SELECT 
    id,
    source_file,
    line_no,
    regexp_split_to_table(
        regexp_replace(raw_line, '^"|"$', 'g'),
        '","'
    ) AS csv_field
FROM orders_import_lines
WHERE source_file = 'orders_dirty.csv';


SELECT *
FROM orders_import_lines
WHERE source_file = 'processor_log.txt'
AND raw_line ~* 'error';


SELECT 
    id,
    source_file,
    line_no,
    regexp_replace(raw_line, 'error', 'ERROR', 'gi') AS normalized_log
FROM orders_import_lines
WHERE source_file = 'processor_log.txt';