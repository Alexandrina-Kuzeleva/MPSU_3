DROP TABLE users;

CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    city VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO users (username, phone, city)
SELECT 
    'user_' || seq,
    '+7' || (floor(random() * 9000000000 + 1000000000))::bigint,
    (ARRAY['Москва', 'Санкт-Петербург', 'Новосибирск', 'Екатеринбург', 'Казань',
        'Челябинск', 'Омск', 'Самара', 'Ростов-на-Дону', 
        'Уфа'])[floor(random() * 10 + 1)]
FROM generate_series(1, 1000000) seq;