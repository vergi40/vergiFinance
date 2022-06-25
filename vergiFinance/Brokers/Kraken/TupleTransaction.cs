namespace vergiFinance.Brokers.Kraken;

/// <summary>
/// Represents complete transactions parsed from 1 or 2 <see cref="RawTransaction"/>
/// </summary>
internal class TupleTransaction
{
    public int DepositCount { get; }
    public int WithdrawalCount { get; }
    public int TransferCount { get; }
    public bool IsTransfer { get; }
    public bool IsTrade { get; }

    public TupleTransaction(RawTransaction transaction1, RawTransaction transaction2)
    {
        var list = new List<RawTransaction> { transaction1, transaction2 };
        foreach (var transaction in list)
        {
            var type = transaction.TypeAsString;
            if (type == "deposit")
            {
                DepositCount++;
            }
            else if (type == "withdrawal")
            {
                WithdrawalCount++;
            }
            else if (type == "transfer")
            {
                TransferCount++;
                IsTransfer = true;
            }
            else if (type is "spend" or "receive" or "trade")
            {
                IsTrade = true;
            }
            else
            {
                throw new NotImplementedException($"Type {type} not implemented");
            }
        }
    }
}