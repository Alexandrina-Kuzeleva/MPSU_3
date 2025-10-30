using Task6.Models;

namespace Task6.OopApproach
{
    public sealed class Luxury : Product
    {
        public Luxury() : base(TaxRates.Luxury) { }
        public override int GetTax(TaxContext context, int price)
        {
            int tax = ApplyModifiers(BaseTax, context);
            return price * tax / 100;
        }
    }
}
