using System;
using Task6.Models;
using Task6.EnumApproach;
using Task6.OopApproach;

namespace Task6
{
    class Program
    {
        static void Main()
        {
            TaxTests.RunAll();
            
            Console.WriteLine("Категория (Food, Clothing, Electronics, Luxury, Digital):");
            string input = Console.ReadLine() ?? "Food";
            Enum.TryParse(input, true, out ProductCategory category);

            Console.Write("Цена: ");
            int price = int.Parse(Console.ReadLine() ?? "0");

            var ctx = new TaxContext
            {
                HasDiscountCard = ReadFlag("Дисконтная карта (true/false): "),
                IsImported = ReadFlag("Импортный товар (true/false): "),
                IsHoliday = ReadFlag("Праздничный день (true/false): ")
            };

            int taxEnum = TaxCalculatorEnum.CalculateTax(category, price, ctx);
            Console.WriteLine($"\n[Enum + switch] Налог: {taxEnum}");

            Product product = category switch
            {
                ProductCategory.Food => new Food(),
                ProductCategory.Clothing => new Clothing(),
                ProductCategory.Electronics => new Electronics(),
                ProductCategory.Luxury => new Luxury(),
                ProductCategory.Digital => new Digital(),
                _ => throw new InvalidOperationException("Неизвестная категория")
            };

            int taxOop = TaxCalculatorOop.CalculateTax(product, ctx, price);
            Console.WriteLine($"[OOP + virtual] Налог: {taxOop}");
        }

        private static bool ReadFlag(string message)
        {
            Console.Write(message);
            return bool.TryParse(Console.ReadLine(), out bool result) && result;
        }
    }
}
