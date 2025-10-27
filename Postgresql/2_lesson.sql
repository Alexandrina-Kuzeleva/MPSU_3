CREATE DATABASE flights;

CREATE TABLE IF NOT EXISTS flights (
    id serial PRIMARY KEY,
    flight_code text NOT NULL CHECK(
        length(flight_code) = 6 
        AND flight_code LIKE 'SU%'
    ),
    duration int NOT NULL CHECK(duration >= 30 AND duration <= 600),
    price int NOT NULL CHECK(price > 100)
);

INSERT INTO flights
    (flight_code, duration, price)
VALUES
    ('SU2345', 120, 110),
    ('SU2945', 333, 747),
    ('SU1448', 500, 22222);