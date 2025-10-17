using Task6.Models;

namespace Task6.OopApproach
{
    public sealed class Digital : Product
    {
        public Digital() : base(TaxRates.Digital) { }
        public override int GetTax(TaxContext context, int price) => 0;
    }
}
