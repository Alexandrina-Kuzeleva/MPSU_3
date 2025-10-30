using Task6.Models;

namespace Task6.EnumApproach
{
    public static class TaxCalculatorEnum
    {
        public static int CalculateTax(
            ProductCategory category, int price, TaxContext context
        )
        {
            int baseTax = category switch
            {
                ProductCategory.Food => TaxRates.Food,
                ProductCategory.Clothing => TaxRates.Clothing,
                ProductCategory.Electronics => TaxRates.Electronics,
                ProductCategory.Luxury => TaxRates.Luxury,
                ProductCategory.Digital => TaxRates.Digital,
                _ => 0
            };

            if (category == ProductCategory.Digital)
                return 0;

            int finalTax = ApplyModifiers(baseTax, context);
            return price * finalTax / 100;
        }

        private static int ApplyModifiers(int tax, TaxContext ctx)
        {
            if (ctx.HasDiscountCard)
                tax = Math.Max(0, tax + TaxRates.DiscountModifier);

            if (ctx.IsImported)
                tax += TaxRates.ImportModifier;

            if (ctx.IsHoliday)
                tax = Math.Max(0, tax + TaxRates.HolidayModifier);

            return tax;
        }
    }
}
