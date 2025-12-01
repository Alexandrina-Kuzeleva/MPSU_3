import asyncio
import asyncpg
from pathlib import Path

async def init_test_db():
    conn = await asyncpg.connect("postgresql://postgres:Red123ty_@localhost:5432/postgres")
    
    try:
        await conn.execute("CREATE DATABASE demo OWNER postgres")
        print("База данных 'demo' создана")
    except asyncpg.exceptions.DuplicateDatabaseError:
        print("База данных 'demo' уже существует")
    finally:
        await conn.close()
    
    conn = await asyncpg.connect("postgresql://postgres:Red123ty_@localhost:5432/demo")
    
    try:
        sql_path = Path(__file__).parent.parent / "sql" / "init.sql"
        
        print(f"Ищем файл по пути: {sql_path}")
        print(f"Файл существует: {sql_path.exists()}")
        
        if not sql_path.exists():
            alternative_paths = [
                Path("sql/init.sql"),
                Path.cwd() / "sql" / "init.sql",
            ]
            
            for alt_path in alternative_paths:
                print(f"Пробуем альтернативный путь: {alt_path}")
                if alt_path.exists():
                    sql_path = alt_path
                    break
        
        if not sql_path.exists():
            raise FileNotFoundError(f"Файл init.sql не найден! Проверяемый путь: {sql_path}")
        
        with open(sql_path, 'r', encoding='utf-8') as f:
            sql_script = f.read()
        
        print(f"Выполняем SQL скрипт из {sql_path}...")
        await conn.execute(sql_script)
        print("Тестовая база данных успешно инициализирована!")
        
    except Exception as e:
        print(f"Ошибка при инициализации БД: {e}")
        raise
    finally:
        await conn.close()

if __name__ == "__main__":
    asyncio.run(init_test_db())