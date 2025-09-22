using System;

namespace ShopProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Система управления магазином ===");
            
            Shop myShop = new Shop("Пятёрка", 200.5);
            
            Product milk = new Product("Молоко", 85.50m, 100);
            Product bread = new Product("Хлеб", 45.00m, 50);
            Product cheese = new Product("Сыр", 320.00m, 30);
            Product eggs = new Product("Яйца", 95.00m, 120);
            
            myShop.AddProduct(milk);
            myShop.AddProduct(bread);
            myShop.AddProduct(cheese);
            myShop.AddProduct(eggs);
            
            Customer customer1 = new Customer("Эйким");
            Customer customer2 = new Customer("Алекс");
            Customer customer3 = new Customer("Костян");
            
            myShop.AddCustomer(customer1);
            myShop.AddCustomer(customer2);
            myShop.AddCustomer(customer3);
            
            Console.WriteLine("\n" + new string('=', 50));
            
            myShop.DisplayShopInfo();
            myShop.DisplayProducts();
            
            Console.WriteLine("\n" + new string('=', 50));
            
            Console.WriteLine("\nПроцесс покупок:");
            myShop.MakePurchase("Эйким", "Молоко", 3);
            myShop.MakePurchase("Эйким", "Хлеб", 2);
            myShop.MakePurchase("Алекс", "Сыр", 1);
            myShop.MakePurchase("Алекс", "Яйца", 10);
            myShop.MakePurchase("Костян", "Молоко", 5);
            myShop.MakePurchase("Костян", "Сыр", 2);
            
            myShop.MakePurchase("Эйким", "Молоко", 1000);
            
            Console.WriteLine("\n" + new string('=', 50));
            
            myShop.DisplayShopInfo();
            myShop.DisplayProducts();
            myShop.DisplayCustomers();
            
            Console.WriteLine("\n" + new string('=', 50));
            
            customer1.DisplayPurchases();
            customer2.DisplayPurchases();
            customer3.DisplayPurchases();
            
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}