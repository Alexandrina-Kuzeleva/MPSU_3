import psycopg2
import time
import json

class PostgreSQLBenchmark:
    def __init__(self, db_config):
        self.conn = psycopg2.connect(**db_config)
        self.cur = self.conn.cursor()

    def execute_and_measure(self, query, params=None):
        start = time.time()
        self.cur.execute(query, params)
        result = self.cur.fetchall()
        end = time.time()
        return result, end - start

    def benchmark_exact_match_phone(self, value):
        query = "SELECT * FROM users WHERE phone = %s"
        _, time_taken = self.execute_and_measure(query, (value,))
        return time_taken

    def benchmark_like_city(self, substring):
        query = "SELECT * FROM users WHERE city ILIKE %s"
        _, time_taken = self.execute_and_measure(query, (f'%{substring}%',))
        return time_taken

    def create_index_phone(self):
        self.cur.execute("CREATE INDEX idx_phone ON users (phone)")
        self.conn.commit()

    def create_expression_index_city(self):
        self.cur.execute(
            "CREATE INDEX idx_city_lower ON users (LOWER(city))"
        )
        self.conn.commit()

    def close(self):
        self.cur.close()
        self.conn.close()

if __name__ == "__main__":
    with open('config.json', 'r', encoding='utf-8') as f:
        config = json.load(f)
    
    db_config = config['db_config']
    test_phone = config['test_values']['phone']
    test_city = config['test_values']['city']

    benchmark = PostgreSQLBenchmark(db_config)

    time_before = benchmark.benchmark_exact_match_phone(test_phone)
    benchmark.create_index_phone()
    time_after = benchmark.benchmark_exact_match_phone(test_phone)
    print(f"Время выборки по phone без индекса: {time_before}, \
          с индексом: {time_after}")

    time_before_like = benchmark.benchmark_like_city(test_city)
    benchmark.create_expression_index_city()
    time_after_like = benchmark.benchmark_like_city(test_city)
    print(f"Время ILIKE по city без индекса: {time_before_like}, \
          с индексом: {time_after_like}")

    benchmark.close()