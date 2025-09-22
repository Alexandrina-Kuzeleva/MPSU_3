class Exhibit:
    def __init__(self, id_number: int, name: str, author: str):
        self.id_number = id_number
        self.name = name
        self.author = author
    
    def __str__(self):
        return f"Экспонат #{self.id_number}: '{self.name}' авторства {self.author}"
    
    def __repr__(self):
        return f"Exhibit(id={self.id_number}, name='{self.name}', author='{self.author}')"


class Hall:
    def __init__(self, name: str, size: float):
        self.name = name
        self.size = size
        self.exhibits = []
    
    def add_exhibit(self, exhibit: Exhibit):
        self.exhibits.append(exhibit)
    
    def remove_exhibit(self, exhibit_id: int):
        self.exhibits = [exh for exh in self.exhibits if exh.id_number != exhibit_id]
    
    def get_exhibits_count(self):
        return len(self.exhibits)
    
    def __str__(self):
        return f"Зал '{self.name}' (площадь: {self.size} м², экспонатов: {self.get_exhibits_count()})"
    
    def __repr__(self):
        return f"Hall(name='{self.name}', size={self.size}, exhibits_count={self.get_exhibits_count()})"


class Museum:
    def __init__(self, name: str, total_area: float):
        self.name = name
        self.total_area = total_area
        self.halls = []
    
    def add_hall(self, hall: Hall):
        self.halls.append(hall)
    
    def remove_hall(self, hall_name: str):
        self.halls = [hall for hall in self.halls if hall.name != hall_name]
    
    def get_total_exhibits_count(self):
        return sum(hall.get_exhibits_count() for hall in self.halls)
    
    def get_occupied_area(self):
        return sum(hall.size for hall in self.halls)
    
    def get_free_area(self):
        return self.total_area - self.get_occupied_area()
    
    def find_exhibit(self, exhibit_id: int):
        for hall in self.halls:
            for exhibit in hall.exhibits:
                if exhibit.id_number == exhibit_id:
                    return exhibit, hall
        return None, None
    
    def __str__(self):
        return (f"Музей '{self.name}'\n"
                f"Общая площадь: {self.total_area} м²\n"
                f"Занято: {self.get_occupied_area()} м²\n"
                f"Свободно: {self.get_free_area()} м²\n"
                f"Количество залов: {len(self.halls)}\n"
                f"Общее количество экспонатов: {self.get_total_exhibits_count()}")
    
    def __repr__(self):
        return f"Museum(name='{self.name}', area={self.total_area}, halls_count={len(self.halls)})"


if __name__ == "__main__":
    exhibit1 = Exhibit(1, "Мона Лиза", "Леонардо да Винчи")
    exhibit2 = Exhibit(2, "Звездная ночь", "Винсент ван Гог")
    exhibit3 = Exhibit(3, "Постоянство памяти", "Сальвадор Дали")
    exhibit4 = Exhibit(4, "Крик", "Эдвард Мунк")
    
    hall1 = Hall("Зал Ренессанса", 150.5)
    hall2 = Hall("Зал Модернизма", 120.3)
    hall3 = Hall("Зал Сюрреализма", 95.7)
    
    hall1.add_exhibit(exhibit1)
    hall2.add_exhibit(exhibit2)
    hall3.add_exhibit(exhibit3)
    hall3.add_exhibit(exhibit4)
    
    museum = Museum("Государственный художественный музей", 500.0)
    
    museum.add_hall(hall1)
    museum.add_hall(hall2)
    museum.add_hall(hall3)
    
    print(museum)
    print("\n" + "="*50 + "\n")
    
    for hall in museum.halls:
        print(hall)
        for exhibit in hall.exhibits:
            print(f"  - {exhibit}")
        print()
    
    found_exhibit, found_hall = museum.find_exhibit(3)
    if found_exhibit:
        print(f"Найден экспонат: {found_exhibit} в зале '{found_hall.name}'")
    
    print(f"\nСвободная площадь музея: {museum.get_free_area():.1f} м²")