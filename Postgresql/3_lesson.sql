CREATE TABLE IF NOT EXISTS books (
    id SERIAL PRIMARY KEY,
    title TEXT NOT NULL UNIQUE CHECK (char_length(title) BETWEEN 2 AND 200),
    author TEXT NOT NULL CHECK (char_length(author) BETWEEN 3 AND 100),
    genre TEXT NOT NULL CHECK (char_length(genre) BETWEEN 3 AND 50),
    price NUMERIC(6,2) NOT NULL CHECK (price > 0 AND price < 1000),
    published_date DATE NOT NULL CHECK (
        published_date BETWEEN '0001-01-01' AND CURRENT_DATE
    )
);


INSERT INTO books (title, author, genre, price, published_date) VALUES
('Dragonfire', 'E. Morgan', 'Fantasy', 12.99, '2015-05-10'),
('Dragonheart', 'K. Lewis', 'Epic Fantasy', 15.50, '2020-12-31'),
('The Last Dragon', 'M. Riley', 'Urban Fantasy', 10.99, '2010-01-01'),
('Dragons of Dawn', 'P. Green', 'Dark Fantasy', 18.99, '2019-03-15'),
('Dragonflight', 'A. McCaffrey', 'Science Fiction', 14.99, '2011-06-01'),

('Space Odyssey', 'A. Clarke', 'Science Fiction', 10.50, '2018-09-01'),
('Galactic Dreams', 'J. Adams', 'Science Fiction', 9.99, '2016-11-20'),
('Star Journey Box Set', 'T. Walker', 'Science Fiction', 15.00, '2019-07-12'),
('Quantum Horizon', 'R. Davis', 'Science Fiction', 19.99, '2014-04-10'),
('Time Rift', 'S. Parker', 'Science Fiction', 20.00, '2013-03-25'),

('Ancient Sample Guide', 'C. Harris', 'Reference', 25.00, '1995-05-01'),
('Old Sample Book', 'J. White', 'Reference', 30.00, '1989-10-12'),
('Modern Reference', 'R. Grant', 'Reference', 28.00, '2000-01-01'),

('Romantic Escape', 'L. Rose', 'Romance', 8.99, '2021-04-17'),
('Murder in Paris', 'D. Noir', 'Mystery', 11.99, '2017-07-25'),
('Lost Galaxy', 'H. Turing', 'Science Fiction', 9.98, '2015-09-09'),
('Fantasy Tales', 'O. Wilde', 'Classic Fantasy', 13.50, '2009-12-31'),
('Dragonfall', 'T. Brooks', 'High Fantasy', 16.49, '2022-01-01'),
('Sample Reference Vol.2', 'M. Kent', 'Fiction', 21.00, '1990-11-11'),
('Dragon Storm', 'E. Hall', 'Heroic Fantasy', 17.99, '2013-08-22'),
('Spacelight', 'I. Reed', 'Science Fiction', 12.99, '2012-05-12'),
('Enchanted Woods', 'F. Moon', 'Fantasy', 14.00, '2018-02-17'),
('The Reference Sample', 'B. King', 'Reference', 35.00, '1980-06-05'),
('Cybernetic Mind', 'J. Black', 'Science Fiction', 18.50, '2019-09-09'),
('The Book of Dreams', 'C. Silver', 'Fantasy', 10.99, '2011-10-11');

SELECT *
FROM books
WHERE genre ILIKE '%fantasy%'
  AND title ILIKE 'Dragon%'
  AND published_date BETWEEN '2010-01-01' AND '2020-12-31'
ORDER BY title;

UPDATE books
SET price = ROUND(price * 1.15, 2)
WHERE genre = 'Science Fiction'
  AND price BETWEEN 9.99 AND 19.99
  AND title NOT ILIKE '%Box Set%';

DELETE FROM books
WHERE genre = 'Reference'
  AND published_date < '2000-01-01'
  AND title ILIKE '%Sample%';

DROP TABLE books;
