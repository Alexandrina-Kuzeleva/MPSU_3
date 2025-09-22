using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopProject
{
    public class Customer
    {
        public string Name { get; private set; }
        public decimal TotalMoneySpent { get; private set; }
        public List<(Product product, int quantity)> PurchasedProducts { get; private set; }

        public Customer(string name)
        {
            Name = name;
            TotalMoneySpent = 0;
            PurchasedProducts = new List<(Product, int)>();
        }

        public bool PurchaseProduct(Product product, int quantity)
        {
            if (product.ReduceStock(quantity))
            {
                decimal totalCost = product.Price * quantity;
                TotalMoneySpent += totalCost;
                
                var existingPurchase = PurchasedProducts.FirstOrDefault(p => p.product.Article == product.Article);
                if (existingPurchase.product != null)
                {
                    PurchasedProducts.Remove(existingPurchase);
                    PurchasedProducts.Add((product, existingPurchase.quantity + quantity));
                }
                else
                {
                    PurchasedProducts.Add((product, quantity));
                }

                Console.WriteLine($"{Name} купил {quantity} шт. товара {product.Article} за {totalCost:C}");
                return true;
            }
            
            Console.WriteLine($"Недостаточно товара {product.Article} на складе. Доступно: {product.StockQuantity} шт.");
            return false;
        }

        public void DisplayPurchases()
        {
            Console.WriteLine($"\nПокупки {Name}:");
            Console.WriteLine($"Всего потрачено: {TotalMoneySpent:C}");
            
            if (PurchasedProducts.Count == 0)
            {
                Console.WriteLine("Покупок нет");
                return;
            }

            foreach (var (product, quantity) in PurchasedProducts)
            {
                Console.WriteLine($"- {product.Article}: {quantity} шт. × {product.Price:C} = {product.Price * quantity:C}");
            }
        }

        public override string ToString()
        {
            return $"Покупатель: {Name}, Потрачено: {TotalMoneySpent:C}, Куплено товаров: {PurchasedProducts.Count}";
        }
    }
}