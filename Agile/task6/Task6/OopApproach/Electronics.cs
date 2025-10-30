using Task6.Models;

namespace Task6.OopApproach
{
    public sealed class Electronics : Product
    {
        public Electronics() : base(TaxRates.Electronics) { }
        public override int GetTax(TaxContext context, int price)
        {
            int tax = ApplyModifiers(BaseTax, context);
            return price * tax / 100;
        }
    }
}
