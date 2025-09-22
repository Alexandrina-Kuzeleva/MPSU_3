class Car:
    def __init__(self, brand, model, mileage=0):
        self.brand = brand
        self.model = model
        self.mileage = mileage
    
    def drive(self, distance):
        if distance > 0:
            self.mileage += distance
            print(f"Автомобиль проехал {distance} км. Новый пробег: {self.mileage} км.")
        else:
            print("Ошибка: дистанция должна быть положительной.")
    
    def __str__(self):
        return f"Автомобиль: {self.brand} {self.model}, Пробег: {self.mileage} км"

def main():
    f1 = Car('Toyta', 'Camry', 14000)    
    f1.drive(12)
    print(f1)

    f1.drive(-100)
    print(f1)

if __name__ == "__main__":
    main()