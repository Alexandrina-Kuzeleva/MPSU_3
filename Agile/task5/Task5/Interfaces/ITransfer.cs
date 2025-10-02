namespace Task5.Interfaces
{
    public interface ITransfer
    {
        int DailyLimit { get; }
        void TransferTo(IAccount target, int amount);
    }
}
