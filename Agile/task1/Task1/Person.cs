using System;

namespace HumanProject
{
    public class Person
    {
        public string Name { get; private set; }
        public int Age { get; private set; }
        public double Height { get; private set; }
        
        public Person(string name, int age, double height)
        {
            Name = name;
            Age = age;
            Height = height;
        }
        public void HaveBirthday()
        {
            Age++;
            Console.WriteLine($"{Name} празднует день рождения! Теперь ему/ей {Age} лет.");
        }
        public override string ToString()
        {
            return $"Человек: {Name}, Возраст: {Age} лет, Рост: {Height} см";
        }
    }
}