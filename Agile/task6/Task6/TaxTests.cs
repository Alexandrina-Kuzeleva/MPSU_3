using System;
using Task6.Models;
using Task6.EnumApproach;
using Task6.OopApproach;

namespace Task6
{
    public static class TaxTests
    {
        private record TestCase(
            ProductCategory Category,
            int Price,
            TaxContext Context,
            int Expected);

        public static void RunAll()
        {
            var tests = new[]
            {
                new TestCase(ProductCategory.Food, 200,
                    new TaxContext(), 5),

                new TestCase(ProductCategory.Electronics, 200,
                    new TaxContext { IsImported = true }, 40),

                new TestCase(ProductCategory.Clothing, 100,
                    new TaxContext { HasDiscountCard = true }, 8),

                new TestCase(ProductCategory.Luxury, 100,
                    new TaxContext { IsHoliday = true }, 22),

                new TestCase(ProductCategory.Digital, 1000,
                    new TaxContext
                    {
                        HasDiscountCard = true,
                        IsImported = true,
                        IsHoliday = true
                    }, 0)
            };

            int passed = 0;
            foreach (var t in tests)
            {
                int resultEnum =
                    TaxCalculatorEnum.CalculateTax(t.Category, t.Price, t.Context);

                Product product = t.Category switch
                {
                    ProductCategory.Food => new Food(),
                    ProductCategory.Clothing => new Clothing(),
                    ProductCategory.Electronics => new Electronics(),
                    ProductCategory.Luxury => new Luxury(),
                    ProductCategory.Digital => new Digital(),
                    _ => throw new InvalidOperationException()
                };

                int resultOop =
                    TaxCalculatorOop.CalculateTax(product, t.Context, t.Price);

                bool ok = resultEnum == t.Expected && resultOop == t.Expected;
                if (ok) passed++;

                Console.WriteLine(
                    $"{t.Category,-12} | Price {t.Price,4} | " +
                    $"Enum: {resultEnum,3} | OOP: {resultOop,3} | " +
                    $"Ожидание: {t.Expected,3} | Результат: {(ok ? "OK" : "FAIL")}");
            }

        }
    }
}
