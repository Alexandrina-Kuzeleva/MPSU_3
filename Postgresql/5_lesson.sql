CREATE DATABASE film_distribution;

DROP TABLE IF EXISTS film_credit CASCADE;
DROP TABLE IF EXISTS film_info CASCADE;
DROP TABLE IF EXISTS film CASCADE;
DROP TABLE IF EXISTS director CASCADE;

CREATE TABLE director (
    id          serial PRIMARY KEY,
    name        text UNIQUE NOT NULL,
    country     text
);

CREATE TABLE film (
    id                   serial PRIMARY KEY,
    title                text NOT NULL,
    release_year         integer CHECK (release_year BETWEEN 1900 AND extract(year from current_date)),
    primary_director_id  integer NOT NULL REFERENCES director(id) ON DELETE RESTRICT
);

CREATE TABLE film_info (
    film_id          integer PRIMARY KEY REFERENCES film(id) ON DELETE CASCADE,
    duration_minutes integer CHECK (duration_minutes > 0),
    rating           text CHECK (rating IN ('G','PG','PG-13','R','NC-17')),
    budget_usd       numeric(15,2) CHECK (budget_usd >= 0)
);

CREATE TABLE film_credit (
    film_id     integer REFERENCES film(id) ON DELETE CASCADE,
    director_id integer REFERENCES director(id),
    role        text NOT NULL,
    PRIMARY KEY (film_id, director_id, role)
);

-- Краткое описание связей
-- 1:N — DIRECTOR → FILM по полю primary_director_id
-- 1:1 — FILM ↔ FILM_INFO по полю film_id (PK + FK)
-- M:N — через FILM_CREDIT (film_id, director_id, role)

INSERT INTO director (name, country) VALUES
('Christopher Nolan', 'UK'),
('Quentin Tarantino', 'USA'),
('Hayao Miyazaki', 'Japan');

INSERT INTO film (title, release_year, primary_director_id) VALUES
('Inception', 2010, 1),
('Pulp Fiction', 1994, 2),
('Spirited Away', 2001, 3);

INSERT INTO film_info (film_id, duration_minutes, rating, budget_usd) VALUES
(1, 148, 'PG-13', 160000000),
(2, 154, 'R', 8000000),
(3, 125, 'PG', 19000000);

INSERT INTO film_credit (film_id, director_id, role) VALUES
(1, 1, 'director'),
(2, 2, 'director'),
(3, 3, 'director'),
(1, 2, 'producer'); 
