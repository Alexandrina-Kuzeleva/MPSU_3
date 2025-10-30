using Task6.Models;

namespace Task6.OopApproach
{
    public abstract class Product
    {
        private readonly int _baseTax;
        
        public int BaseTax => _baseTax;
        
        protected Product(int baseTax) => _baseTax = baseTax;

        public virtual int GetTax(TaxContext context, int price)
        {
            return price * _baseTax / 100;
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
