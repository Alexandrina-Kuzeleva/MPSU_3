using Task6.Models;

namespace Task6.OopApproach
{
    public abstract class Product
    {
        protected readonly int baseTax;
        protected Product(int baseTax) => this.baseTax = baseTax;

        public virtual int GetTax(TaxContext context, int price)
        {
            return price * baseTax / 100;
        }

        protected static int ApplyModifiers(int tax, TaxContext ctx)
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
