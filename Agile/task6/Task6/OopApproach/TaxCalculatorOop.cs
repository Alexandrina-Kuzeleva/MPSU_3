namespace Task6.OopApproach
{
    public static class TaxCalculatorOop
    {
        public static int CalculateTax(Product product, 
                                       Models.TaxContext context, 
                                       int price)
        {
            return product.GetTax(context, price);
        }
    }
}
