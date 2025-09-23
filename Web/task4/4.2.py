import time
from functools import wraps

def retry(times=3, exceptions=(Exception,), delay=0.0):
    if not isinstance(times, int) or times < 1:
        raise ValueError("times должен быть целым числом больше 0")
    
    if not isinstance(exceptions, tuple) or not all(issubclass(e, BaseException) for e in exceptions):
        raise ValueError("exceptions должен быть кортежем классов исключений")
    
    if not isinstance(delay, (int, float)) or delay < 0:
        raise ValueError("delay должен быть неотрицательным числом")
    
    def decorator(func):
        @wraps(func)
        def wrapper(*args, **kwargs):
            last_exception = None
            
            for attempt in range(1, times + 1):
                try:
                    result = func(*args, **kwargs)
                    print(f"Успех на попытке {attempt}/{times}")
                    return result
                except exceptions as e:
                    last_exception = e
                    print(f"Попытка {attempt}/{times} не удалась: {e}")
                    
                    if attempt == times:
                        break
                    
                    if delay > 0:
                        print(f"Ожидание {delay} сек...")
                        time.sleep(delay)
                    print("Повторная попытка...")
            
            print(f"Все {times} попыток исчерпаны")
            raise last_exception
        return wrapper
    return decorator

print("Пример 1")
i = 0

@retry(times=4, exceptions=(ValueError,), delay=0.1)
def flaky():
    global i
    i += 1
    if i < 3:
        raise ValueError("not yet")
    return "ok"

result = flaky()
print(f"Результат: {result}\n")

print("Пример 2")

@retry(times=3, exceptions=(RuntimeError,), delay=0.2)
def always_fails():
    raise RuntimeError("Unavailable")

try:
    always_fails()
except Exception as e:
    print(f"Exception: {e}")