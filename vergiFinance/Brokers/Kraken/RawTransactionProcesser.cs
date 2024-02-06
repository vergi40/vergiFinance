using vergiFinance.Model;

namespace vergiFinance.Brokers.Kraken
{
    internal class RawTransactionProcesser
    {
        public List<TransactionBase> ProcessRawTransactions(IReadOnlyList<RawTransaction> transactions)
        {
            var result = new List<TransactionBase>();
            var (singles, pairs) = CombineTransactions(transactions);

            // 2x withdrawal for bank - kraken transactions
            // withdrawal+transfer for staking

            // 2x deposit for bank - kraken transactions
            // deposit+transfer for staking

            // staking - has unique deposit and unique staking event
            //   deposit currency != ZEUR

            foreach (var singleEvent in singles)
            {
                if (singleEvent.TypeAsString == "staking")
                {
                    result.Add(TransactionFactory.Create(TransactionType.StakingOperation, FiatCurrency.Eur, singleEvent.Asset,
                        Math.Abs(singleEvent.Amount), 1m, singleEvent.Time));
                }
                else if (singleEvent.TypeAsString == "deposit")
                {
                    result.Add(TransactionFactory.Create(TransactionType.StakingDividend, FiatCurrency.Eur, singleEvent.Asset,
                        Math.Abs(singleEvent.Amount), 1m, singleEvent.Time));
                }
                else
                {
                    throw new NotImplementedException($"Type {singleEvent.TypeAsString} transaction not implemented");
                }
            }

            foreach (var pair in pairs)
            {
                var (item1, item2) = pair;
                var info = new TupleTransaction(item1, item2);
                if (info.DepositCount == 2)
                {
                    result.Add(TransactionFactory.Create(TransactionType.Deposit, FiatCurrency.Eur, "",
                        Math.Abs(item1.Amount), 1m, item1.Time));
                }
                else if (info.WithdrawalCount == 2)
                {
                    result.Add(TransactionFactory.Create(TransactionType.Withdrawal, FiatCurrency.Eur, "",
                        Math.Abs(item1.Amount), 1m, item1.Time));
                }
                else if (info.IsTransfer)
                {
                    if (info.WithdrawalCount == 1)
                    {
                        result.Add(TransactionFactory.CreateStakingTransfer(TransactionType.StakingWithdrawal, item1, item2));
                    }
                    else if (info.DepositCount == 1)
                    {
                        result.Add(TransactionFactory.CreateStakingTransfer(TransactionType.StakingDeposit, item1, item2));
                    }
                }
                else if (info.IsTrade)
                {
                    result.Add(TransactionFactory.CreateKrakenTrade(item1, item2));
                }
            }
            return result;
        }

        /// <summary>
        /// Intermediate step to simplify raw transactions and combine pairs
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private (IReadOnlyList<RawTransaction> singles, IReadOnlyList<(RawTransaction, RawTransaction)> pairs) CombineTransactions(
            IReadOnlyList<RawTransaction> transactions)
        {
            var dict = new Dictionary<string, List<RawTransaction>>();
            foreach (var transaction in transactions)
            {
                if (dict.ContainsKey(transaction.ReferenceId))
                {
                    dict[transaction.ReferenceId].Add(transaction);
                }
                else
                {
                    dict.Add(transaction.ReferenceId, new List<RawTransaction> { transaction });
                }
            }

            var singles = dict.Values.Where(v => v.Count == 1).Select(v => v.Single()).ToList();
            var pairs = dict.Values.Where(v => v.Count == 2).ToList();

            if (dict.Values.Any(v => v.Count > 2))
            {
                throw new NotImplementedException("Unrecognized transaction - has more than 2 events with same reference id");
            }

            return (singles, pairs.Select(p => (p[0], p[1])).ToList());
        }
    }
}
