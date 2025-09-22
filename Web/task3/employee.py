class Employee:
    def __init__(self, name, salary):
        self._validate_name(name)
        self._validate_salary(salary)
        self.name = name
        self.salary = salary
    
    def _validate_name(self, name):
        if not isinstance(name, str) or len(name.strip()) == 0:
            raise ValueError("Имя сотрудника должно быть непустой строкой")
    
    def _validate_salary(self, salary):
        if not isinstance(salary, (int, float)) or salary <= 0:
            raise ValueError("Зарплата должна быть положительным числом")
    
    def calculate_bonus(self):
        return self.salary * 0.10
    
    def __str__(self):
        return f"{self.__class__.__name__}: {self.name}, Зарплата: {self.salary} руб."

class Manager(Employee):
    def __init__(self, name, salary, management_level):
        super().__init__(name, salary)
        self._validate_management_level(management_level)
        self.management_level = management_level
    
    def _validate_management_level(self, level):
        if not isinstance(level, int) or level <= 0:
            raise ValueError("Уровень руководства должен быть положительным целым числом")
    
    def calculate_bonus(self):
        base_bonus = super().calculate_bonus()
        if self.management_level > 3:
            return base_bonus + (self.salary * 0.05)
        return base_bonus
    
    def __str__(self):
        return f"{super().__str__()}, Уровень: {self.management_level}"


class Developer(Employee):
    def __init__(self, name, salary, know_javascript):
        super().__init__(name, salary)
        self._validate_know_javascript(know_javascript)
        self.know_javascript = know_javascript
    
    def _validate_know_javascript(self, know_js):
        if not isinstance(know_js, bool):
            raise ValueError("Знание JavaScript должно быть булевым значением (True/False)")
    
    def calculate_bonus(self):
        base_bonus = super().calculate_bonus()
        if self.know_javascript:
            return base_bonus + (self.salary * 0.03)
        return base_bonus
    
    def __str__(self):
        js_status = "владеет" if self.know_javascript else "не владеет"
        return f"{super().__str__()}, {js_status} JavaScript"

def test_employees():
    print("=== Тестирование сотрудников ===")
    
    try:
        manager = Manager("Иван Иванов", 100000, 4)
        developer = Developer("Петр Петров", 80000, True)
        
        print("Сотрудники созданы успешно")
        print(manager)
        print(developer)
        
    except ValueError as e:
        print(f"✗ Ошибка при создании сотрудников: {e}")
        return
    
    try:
        manager_bonus = manager.calculate_bonus()
        developer_bonus = developer.calculate_bonus()
        
        print(f"Премии рассчитаны успешно:")
        print(f"Менеджер премия: {manager_bonus} руб.")
        print(f"Разработчик премия: {developer_bonus} руб.")
        
    except Exception as e:
        print(f"Ошибка при расчете премий: {e}")
        return
    
    print("Тестирование валидации ===")
    
    test_cases = [
        ("Пустое имя", lambda: Manager("", 100000, 4)),
        ("Отрицательная зарплата", lambda: Developer("Тест", -50000, True)),
        ("Нулевая зарплата", lambda: Manager("Тест", 0, 2)),
        ("Некорректный уровень", lambda: Manager("Тест", 50000, -1)),
        ("Небулевое знание JS", lambda: Developer("Тест", 50000, "да")),
    ]
    
    for test_name, test_func in test_cases:
        try:
            test_func()
            print(f"{test_name}: Ожидалась ошибка, но создание прошло успешно")
        except ValueError as e:
            print(f"{test_name}: Корректно отловлена ошибка - {e}")
        except Exception as e:
            print(f"{test_name}: Неожиданная ошибка - {e}")

if __name__ == "__main__":
    test_employees()
    
    manager = Manager("Иван Иванов", 100000, 4)
    developer = Developer("Петр Петров", 80000, True)
    
    manager_bonus = manager.calculate_bonus()
    developer_bonus = developer.calculate_bonus()
    
    print(f"Менеджер {manager.name}:")
    print(f"Зарплата: {manager.salary} руб.")
    print(f"Уровень руководства: {manager.management_level}")
    print(f"Премия: {manager_bonus} руб. ({manager_bonus/manager.salary*100:.1f}%)")
    print(f"Общий доход: {manager.salary + manager_bonus} руб.")
    print()
    
    print(f"Разработчик {developer.name}:")
    print(f"Зарплата: {developer.salary} руб.")
    print(f"Владеет JavaScript: {'Да' if developer.know_javascript else 'Нет'}")
    print(f"Премия: {developer_bonus} руб. ({developer_bonus/developer.salary*100:.1f}%)")
    print(f"Общий доход: {developer.salary + developer_bonus} руб.")