using System;

namespace HumanProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Демонстрация работы класса Человек ===");
            Console.WriteLine();

            Person person1 = new Person("Костя", 20, 179.5);
            Person person2 = new Person("Аким", 19, 182.0);
            Person person3 = new Person("Алекс", 20, 180.2);

            Console.WriteLine("Созданные люди:");
            Console.WriteLine(person1);
            Console.WriteLine(person2);
            Console.WriteLine(person3);
            Console.WriteLine();

            Console.WriteLine("Празднуем дни рождения:");
            person1.HaveBirthday();
            person2.HaveBirthday();
            person2.HaveBirthday(); 
            Console.WriteLine();

            Console.WriteLine("Обновленная информация:");
            Console.WriteLine(person1);
            Console.WriteLine(person2);
            Console.WriteLine(person3); 
            Console.WriteLine();

            Console.WriteLine("Нажмите клавишу для выхода");
            Console.ReadKey();
        }
    }
}