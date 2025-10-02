namespace Task5.Interfaces
{
    public interface IAccount
    {
        string OwnerName { get; }
        int Balance { get; }

        void Withdraw(int amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Сумма снятия должна быть положительной.");
                return;
            }
            if (amount > Balance)
            {
                Console.WriteLine("Запрошенная сумма превышает баланс, списано всё до 0");
                SetBalance(0);
            }
            else
            {
                SetBalance(Balance - amount);
            }
        }

        void Deposit(int amount)
        {
            if (amount > 0)
            {
                SetBalance(Balance + amount);
            }
        }

        void SetBalance(int value);
    }
}
