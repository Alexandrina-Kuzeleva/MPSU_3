using System;

namespace ApplianceTask
{
    public class Appliance
    {
        private string _applianceMake;
        private int _consumptionWatts;

        public string ApplianceMake
        {
            get { return _applianceMake; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("Марка не может быть пустой");
                }
                if (value.Length > 50)
                {
                    throw new Exception("Слишком длинное название марки");
                }
                
                _applianceMake = value.Trim();
            }
        }

        public int ConsumptionWatts
        {
            get { return _consumptionWatts; }
            set
            {
                if (value <= 0)
                {
                    throw new Exception("Мощность должна быть больше нуля");
                }
                
                if (value > 10000)
                {
                    throw new Exception("Слишком большая мощность");
                }
                
                _consumptionWatts = value;
            }
        }

        public Appliance(string make, int power)
        {
            ApplianceMake = make;
            ConsumptionWatts = power;
        }

        public virtual void UsageInfo()
        {
            Console.WriteLine($"Стиральная машина марки {ApplianceMake} осуществляет стирку в цикле {ConsumptionWatts}.");
        }
    }

    public class Refrigerator : Appliance
    {
        private string _temperatureMode;

        public string TemperatureMode
        {
            get { return _temperatureMode; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("Режим температуры не может быть пустым");
                }
                
                bool valid = true;
                foreach (char c in value)
                {
                    if (!char.IsDigit(c) && c != '+' && c != '-' && c != '.' && c != ',')
                    {
                        valid = false;
                        break;
                    }
                }
                
                if (!valid)
                {
                    throw new Exception("Неправильные символы в режиме температуры");
                }
                
                _temperatureMode = value.Trim();
            }
        }

        public Refrigerator(string make, int power, string temp) : base(make, power)
        {
            TemperatureMode = temp;
        }

        public override void UsageInfo()
        {
            Console.WriteLine($"Холодильник марки {ApplianceMake} поддерживает температуру {TemperatureMode} градусов Цельсия.");
        }
    }

    public class WashingMachine : Appliance
    {
        private string _washCycle;

        public string WashCycle
        {
            get { return _washCycle; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("Цикл стирки не может быть пустым");
                }
                
                if (value.Length > 100)
                {
                    throw new Exception("Слишком длинное название цикла");
                }
                
                _washCycle = value.Trim();
            }
        }

        public WashingMachine(string make, int power, string cycle) : base(make, power)
        {
            WashCycle = cycle;
        }

        public override void UsageInfo()
        {
            Console.WriteLine($"Стиральная машина {ApplianceMake}, Цикл: {WashCycle}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Тестирование бытовой техники");
            Console.WriteLine();

            try
            {
                Console.WriteLine("1. Создаем холодильник:");
                Refrigerator fridge1 = new Refrigerator("Samsung", 200, "+4");
                fridge1.UsageInfo();
                Console.WriteLine();

                Console.WriteLine("2. Создаем стиральную машину:");
                WashingMachine washer1 = new WashingMachine("LG", 800, "Хлопок");
                washer1.UsageInfo();
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
            }

            Console.WriteLine("3. Тест ошибок:");
            Console.WriteLine();

            try
            {
                Console.WriteLine("Пытаемся создать с пустой маркой:");
                Refrigerator fridge2 = new Refrigerator("", 150, "+4");
            }
            catch (Exception e)
            {
                Console.WriteLine("Случилась ошибка: " + e.Message);
            }
            Console.WriteLine();

            try
            {
                Console.WriteLine("Пытаемся создать с отрицательной мощностью:");
                WashingMachine washer2 = new WashingMachine("Bosch", -100, "Шерсть");
            }
            catch (Exception e)
            {
                Console.WriteLine("Случилась ошибка: " + e.Message);
            }
            Console.WriteLine();

            try
            {
                Console.WriteLine("Пытаемся создать с неправильной температурой:");
                Refrigerator fridge3 = new Refrigerator("Atlant", 180, "холодно");
            }
            catch (Exception e)
            {
                Console.WriteLine("Случилась ошибка: " + e.Message);
            }
            Console.WriteLine();

            try
            {
                Console.WriteLine("Пытаемся создать с огромной мощностью:");
                WashingMachine washer3 = new WashingMachine("Mega", 50000, "Супер");
            }
            catch (Exception e)
            {
                Console.WriteLine("Случилась ошибка: " + e.Message);
            }
            Console.WriteLine();

            Console.WriteLine("Тестирование завершено!");
            Console.ReadLine();
        }
    }
}