using vergiFinance.Model;

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
    public TransactionType StakeTransferType { get; set; } = TransactionType.Noop;

    public TupleTransaction(RawTransaction transaction1, RawTransaction transaction2)
    {
        foreach (var transaction in new List<RawTransaction> { transaction1, transaction2 })
        {
            var transactionType = transaction.TypeAsString;
            if (transactionType == "deposit")
            {
                DepositCount++;
            }
            else if (transactionType == "withdrawal")
            {
                WithdrawalCount++;
            }
            else if (transactionType == "transfer")
            {
                TransferCount++;
                IsTransfer = true;
                if (transaction.SubType == "spottostaking" || transaction.SubType == "stakingfromspot")
                {
                    StakeTransferType = TransactionType.WalletToStaking;
                }
                else if (transaction.SubType == "stakingtospot" || transaction.SubType == "spotfromstaking")
                {
                    StakeTransferType = TransactionType.StakingToWallet;
                }
            }
            else if (transactionType is "spend" or "receive" or "trade")
            {
                IsTrade = true;
            }
            else
            {
                throw new NotImplementedException($"Type {transactionType} not implemented");
            }
        }
    }
}