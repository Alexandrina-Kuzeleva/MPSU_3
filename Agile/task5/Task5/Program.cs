using System;
using Task5.Interfaces;
using Task5.Models;

namespace Task5
{
    class Program
    {
        static void Main()
        {
            IAccount savings = new SavingsAccount("Алексей", 1000);
            savings.Withdraw(1500);
            Console.WriteLine(
                $"Сберегательный счёт {savings.OwnerName}, баланс: {savings.Balance}"
            );

            PremiumAccount premium = new PremiumAccount("Мария", 2000, 500);
            premium.Withdraw(1000);
            Console.WriteLine(
                $"Премиум-счёт {premium.OwnerName}, баланс после withdraw: {premium.Balance}"
            );

            SavingsAccount target = new SavingsAccount("Иван", 300);
            premium.TransferTo(target, 800);
            Console.WriteLine(
                $"Премиум-счёт {premium.OwnerName}, баланс после transfer: {premium.Balance}"
            );
            Console.WriteLine(
                $"Счёт {target.OwnerName}, баланс после получения: {target.Balance}"
            );
        }
    }
}
