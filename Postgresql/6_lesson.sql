CREATE DATABASE trips_database;

-- Схема
DROP TABLE IF EXISTS trips;
CREATE TABLE trips (
  trip_id        SERIAL PRIMARY KEY,
  trip_date      DATE        NOT NULL,
  city           TEXT        NOT NULL CHECK (city IN ('Stockholm', 'Gothenburg', 'Malmo', 'Uppsala')),
  rider_type     TEXT        NOT NULL CHECK (rider_type IN ('new', 'loyal', 'corporate')),
  payment_method TEXT        NOT NULL CHECK (payment_method IN ('card', 'cash', 'wallet')),
  status         TEXT        NOT NULL CHECK (status IN ('requested', 'accepted', 'completed', 'cancelled', 'refunded')),
  distance_km    NUMERIC(5,2) NOT NULL CHECK (distance_km > 0),
  base_fare      NUMERIC(6,2) NOT NULL CHECK (base_fare >= 0),
  surge          NUMERIC(3,2) NOT NULL CHECK (surge BETWEEN 1.0 AND 2.5),
  tip            NUMERIC(5,2) NOT NULL DEFAULT 0 CHECK (tip >= 0),
  discount       NUMERIC(5,2) NOT NULL DEFAULT 0 CHECK (discount >= 0)
);

-- Наполнение (28 строк)
INSERT INTO trips (trip_date,city,rider_type,payment_method,status,distance_km,base_fare,surge,tip,discount) VALUES
('2025-01-03','Stockholm','new','card','completed',   4.2, 120, 1.2,  10,  0),
('2025-01-04','Stockholm','loyal','wallet','completed',7.8, 150, 1.0,   0, 10),
('2025-01-05','Stockholm','corporate','card','cancelled',3.1,100, 1.1,   0,  0),
('2025-01-07','Stockholm','new','cash','completed',   2.6,  90, 1.3,   5,  0),
('2025-01-10','Stockholm','loyal','card','refunded',  5.0, 140, 1.1,   0,  0),
('2025-02-02','Gothenburg','new','card','completed',  6.5, 130, 1.5,   8,  0),
('2025-02-03','Gothenburg','loyal','wallet','completed',9.4,170, 1.2,   0, 15),
('2025-02-04','Gothenburg','corporate','card','completed',11.0,220,1.1,  0,  0),
('2025-02-06','Gothenburg','new','cash','cancelled',  1.8,  80, 1.0,   0,  0),
('2025-02-09','Gothenburg','loyal','card','completed', 3.9,  95, 1.6,   4,  0),
('2025-03-01','Malmo','new','wallet','completed',     5.7, 125, 1.4,   6,  5),
('2025-03-02','Malmo','loyal','card','completed',     8.2, 160, 1.1,   0,  0),
('2025-03-03','Malmo','corporate','cash','completed', 12.6,240, 1.3,   0,  0),
('2025-03-05','Malmo','new','card','cancelled',       2.0,  85, 1.0,   0,  0),
('2025-03-07','Malmo','loyal','wallet','refunded',    6.0, 135, 1.2,   0,  0),
('2025-03-10','Uppsala','new','card','completed',     3.3,  95, 1.0,   2,  0),
('2025-03-11','Uppsala','loyal','cash','completed',   7.1, 155, 1.3,   0, 10),
('2025-03-12','Uppsala','corporate','card','completed',10.4,210,1.5,   0,  0),
('2025-03-14','Uppsala','new','wallet','cancelled',   1.5,  70, 1.0,   0,  0),
('2025-03-16','Uppsala','loyal','card','completed',   4.8, 110, 1.2,   3,  0),
('2025-04-01','Stockholm','corporate','card','completed',13.2,260,1.4,  0,  0),
('2025-04-02','Stockholm','new','wallet','completed',  6.9,145, 1.1,   5,  0),
('2025-04-03','Stockholm','loyal','card','completed',  9.0,175, 1.7,   0, 20),
('2025-04-04','Gothenburg','corporate','wallet','refunded',9.2,200,1.2, 0,  0),
('2025-04-06','Gothenburg','new','card','completed',   5.4,120, 1.8,   7,  0),
('2025-04-07','Malmo','loyal','card','completed',      7.6,150, 1.0,   0, 10),
('2025-04-08','Malmo','new','cash','completed',        3.1, 90,  1.9,  3,  0),
('2025-04-09','Uppsala','corporate','card','cancelled', 2.2,100, 1.0,  0,  0);


SELECT 
    trip_id,
    city,
    status,
    CASE 
        WHEN status = 'completed' THEN base_fare * surge + tip - discount 
        ELSE 0 
    END AS net_charge,
    CASE 
        WHEN distance_km <= 4 THEN 'short'
        WHEN distance_km <= 8 THEN 'mid' 
        ELSE 'long' 
    END AS distance_band
FROM trips
ORDER BY trip_date, trip_id;


SELECT 
    trip_id,
    rider_type,
    distance_km
FROM trips
WHERE distance_km > CASE 
    WHEN rider_type = 'new' THEN 5 
    WHEN rider_type = 'loyal' THEN 8 
    ELSE 10 
END;


SELECT 
    city,
    COUNT(*) AS trips_cnt,
    SUM(distance_km) AS sum_distance,
    AVG(surge) AS avg_surge,
    MAX(tip) AS max_tip
FROM trips
GROUP BY city
ORDER BY sum_distance DESC;


SELECT 
    city,
    SUM(base_fare * surge + tip - discount) FILTER (WHERE status = 'completed') AS delivered_revenue,
    COUNT(*) FILTER (WHERE status = 'cancelled') AS cancel_cnt,
    COUNT(*) FILTER (WHERE status = 'cancelled')::NUMERIC / COUNT(*) AS cancel_rate
FROM trips
GROUP BY city
ORDER BY cancel_rate DESC;


SELECT 
    payment_method,
    COUNT(*) AS total_trips,
    COUNT(*) FILTER (WHERE status = 'cancelled') AS cancel_cnt,
    SUM(base_fare * surge + tip - discount) FILTER (WHERE status = 'completed') AS delivered_revenue
FROM trips
GROUP BY payment_method
HAVING 
    COUNT(*) FILTER (WHERE status = 'cancelled')::NUMERIC / COUNT(*) > 0.15
    OR SUM(base_fare * surge + tip - discount) FILTER (WHERE status = 'completed') > 1200;