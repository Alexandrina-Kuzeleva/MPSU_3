using System;

namespace ShopProject
{
    public class Product
    {
        public string Article { get; private set; }
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }

        public Product(string article, decimal price, int stockQuantity)
        {
            Article = article;
            Price = price;
            StockQuantity = stockQuantity;
        }

        public bool ReduceStock(int quantity)
        {
            if (quantity <= StockQuantity)
            {
                StockQuantity -= quantity;
                return true;
            }
            return false;
        }

        public void IncreaseStock(int quantity)
        {
            StockQuantity += quantity;
        }

        public override string ToString()
        {
            return $"Артикул: {Article}, Цена: {Price:C}, Остаток: {StockQuantity} шт.";
        }
    }
}