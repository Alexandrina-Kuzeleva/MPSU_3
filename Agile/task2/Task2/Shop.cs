using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopProject
{
    public class Shop
    {
        public string Name { get; private set; }
        public double Area { get; private set; }
        public List<Product> Products { get; private set; }
        public List<Customer> Customers { get; private set; }

        public Shop(string name, double area)
        {
            Name = name;
            Area = area;
            Products = new List<Product>();
            Customers = new List<Customer>();
        }

        public void AddProduct(Product product)
        {
            Products.Add(product);
            Console.WriteLine($"Товар {product.Article} добавлен в магазин {Name}");
        }

        public void AddCustomer(Customer customer)
        {
            Customers.Add(customer);
            Console.WriteLine($"Покупатель {customer.Name} зарегистрирован в магазине {Name}");
        }

        public Product FindProduct(string article)
        {
            return Products.FirstOrDefault(p => p.Article == article);
        }

        public bool MakePurchase(string customerName, string productArticle, int quantity)
        {
            var customer = Customers.FirstOrDefault(c => c.Name == customerName);
            var product = FindProduct(productArticle);

            if (customer == null)
            {
                Console.WriteLine($"Покупатель {customerName} не найден");
                return false;
            }

            if (product == null)
            {
                Console.WriteLine($"Товар {productArticle} не найден");
                return false;
            }

            return customer.PurchaseProduct(product, quantity);
        }

        public void DisplayShopInfo()
        {
            Console.WriteLine($"\n=== Магазин: {Name} ===");
            Console.WriteLine($"Площадь: {Area} м²");
            Console.WriteLine($"Количество товаров: {Products.Count}");
            Console.WriteLine($"Количество покупателей: {Customers.Count}");
            
            decimal totalInventoryValue = Products.Sum(p => p.Price * p.StockQuantity);
            Console.WriteLine($"Общая стоимость товаров на складе: {totalInventoryValue:C}");
        }

        public void DisplayProducts()
        {
            Console.WriteLine($"\nАссортимент магазина {Name}:");
            if (Products.Count == 0)
            {
                Console.WriteLine("Товаров нет");
                return;
            }

            foreach (var product in Products)
            {
                Console.WriteLine($"- {product}");
            }
        }

        public void DisplayCustomers()
        {
            Console.WriteLine($"\nПокупатели магазина {Name}:");
            if (Customers.Count == 0)
            {
                Console.WriteLine("Покупателей нет");
                return;
            }

            foreach (var customer in Customers.OrderByDescending(c => c.TotalMoneySpent))
            {
                Console.WriteLine($"- {customer}");
            }
        }

        public override string ToString()
        {
            return $"Магазин: {Name}, Площадь: {Area} м², Товаров: {Products.Count}, Покупателей: {Customers.Count}";
        }
    }
}