using System;
using Task5.Interfaces;

namespace Task5.Models
{
    public class SavingsAccount : IAccount
    {
        private string _ownerName;
        private int _balance;

        public string OwnerName => _ownerName;
        public int Balance => _balance;

        public SavingsAccount(string ownerName, int initialBalance)
        {
            if (string.IsNullOrWhiteSpace(ownerName))
                throw new ArgumentException("Имя владельца не может быть пустым.");

            _ownerName = ownerName;
            _balance = initialBalance < 0 ? 0 : initialBalance;
        }

        void IAccount.SetBalance(int value) => _balance = value;
    }
}
