from functools import wraps

def enforce_ints(strict=False):
    def decorator(func):
        @wraps(func)
        def wrapper(*args, **kwargs):
            for i, arg in enumerate(args):
                if strict:
                    if type(arg) is not int:
                        raise TypeError(f"Аргумент #{i+1} должен быть int, получен {type(arg).__name__}")
                else:
                    if not isinstance(arg, int):
                        raise TypeError(f"Аргумент #{i+1} должен быть целым числом, получен {type(arg).__name__}")
                    if isinstance(arg, bool):
                        print(f"Предупреждение: аргумент #{i+1} является bool, а не int")
            
            return func(*args, **kwargs)
        return wrapper
    return decorator

@enforce_ints(strict=False)
def mul(a, b):
    return a * b

@enforce_ints(strict=True)
def add(a, b):
    return a + b

@enforce_ints(strict=False)
def power(a, b):
    return a ** b

def main():
    print("Нестрогий режим (strict=False)")
    print("mul(2, 5) =", mul(2, 5))  # 10
    
    try:
        print('mul(2, "5") =', end=" ")
        print(mul(2, "5"))
    except TypeError as e:
        print(f"Ошибка: {e}")
    
    print("mul(True, 5) =", end=" ")
    result = mul(True, 5)
    print(result)
    
    print("Строгий режим (strict=True)")
    print("add(3, 7) =", add(3, 7))  
    
    try:
        print("add(True, 5) =", end=" ")
        print(add(True, 5)) 
    except TypeError as e:
        print(f"Ошибка: {e}")
    
    try:
        print('add(2, "5") =', end=" ")
        print(add(2, "5"))  
    except TypeError as e:
        print(f"Ошибка: {e}")
    
    print("Именованные аргументы (не проверяются)")
    print("mul(a=2, b=5) =", mul(a=2, b=5)) 
    
    print("Дополнительные тесты")
    print("power(2, 3) =", power(2, 3)) 
    
    print("power(True, 3) =", end=" ")
    result = power(True, 3)
    print(result)


if __name__ == "__main__":
    main()