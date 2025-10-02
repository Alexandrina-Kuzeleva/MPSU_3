using System;
using Task5.Interfaces;

namespace Task5.Models
{
    public class PremiumAccount : IAccount, ITransfer
    {
        private string _ownerName;
        private int _balance;
        private int _dailyLimit;

        public string OwnerName => _ownerName;
        public int Balance => _balance;
        public int DailyLimit => _dailyLimit;

        public PremiumAccount(string ownerName, int initialBalance, int dailyLimit)
        {
            if (string.IsNullOrWhiteSpace(ownerName))
                throw new ArgumentException("Имя владельца не может быть пустым.");
            if (dailyLimit <= 0)
                throw new ArgumentException("Лимит должен быть положительным.");

            _ownerName = ownerName;
            _balance = initialBalance < 0 ? 0 : initialBalance;
            _dailyLimit = dailyLimit;
        }

        public void Withdraw(int amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Сумма снятия должна быть положительной.");
                return;
            }

            if (amount > _dailyLimit)
            {
                Console.WriteLine($"Запрошенная сумма превышает дневной лимит {_dailyLimit}, будет снято только {_dailyLimit}.");
            }

            int toWithdraw = amount > _dailyLimit ? _dailyLimit : amount;

            if (toWithdraw > Balance)
            {
                Console.WriteLine("Запрошенная сумма превышает баланс, списано всё до 0");
                ((IAccount)this).SetBalance(0);
            }
            else
            {
                ((IAccount)this).SetBalance(Balance - toWithdraw);
            }
        }

        public void TransferTo(IAccount target, int amount)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            int before = _balance;
            Withdraw(amount);
            int actuallyWithdrawn = before - _balance;

            if (actuallyWithdrawn > 0)
            {
                target.Deposit(actuallyWithdrawn);
            }
        }

        void IAccount.SetBalance(int value) => _balance = value;
    }
}
