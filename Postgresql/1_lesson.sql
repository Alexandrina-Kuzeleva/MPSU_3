CREATE SCHEMA fitness_center;

CREATE TABLE fitness_center.members (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    registration_date DATE DEFAULT CURRENT_DATE
);

CREATE TABLE fitness_center.trainers (
    id INTEGER PRIMARY KEY,
    name TEXT UNIQUE NOT NULL,
    specialization TEXT NOT NULL,
    experience_years INTEGER NOT NULL
);

SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'fitness_center';