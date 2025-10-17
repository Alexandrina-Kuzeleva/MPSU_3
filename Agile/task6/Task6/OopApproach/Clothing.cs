using Task6.Models;

namespace Task6.OopApproach
{
    public sealed class Clothing : Product
    {
        public Clothing() : base(TaxRates.Clothing) { }
        public override int GetTax(TaxContext context, int price)
        {
            int tax = ApplyModifiers(baseTax, context);
            return price * tax / 100;
        }
    }
}
