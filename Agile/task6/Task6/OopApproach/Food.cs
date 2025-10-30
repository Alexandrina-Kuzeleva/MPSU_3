using Task6.Models;

namespace Task6.OopApproach
{
    public sealed class Food : Product
    {
        public Food() : base(TaxRates.Food) { }
        public override int GetTax(TaxContext context, int price)
        {
            int tax = ApplyModifiers(BaseTax, context);
            return price * tax / 100;
        }
    }
}
