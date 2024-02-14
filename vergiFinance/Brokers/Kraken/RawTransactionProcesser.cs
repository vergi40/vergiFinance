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

            // staking
            // Deposit from wallet: withdrawal+transfer TRX (negative). deposit+transfer TRX.S
            // Withdrawal to wallet: withdrawal+transfer TRX.S (negative). deposit+transfer TRX
            // Dividends: deposit+staking (non-linked) TRX.S

            foreach (var singleEvent in singles)
            {
                if (singleEvent.TypeAsString == "deposit")
                {
                    // Staking single operation "start"
                    // Can be skipped as duplicate with "staking" event
                    continue;
                    //result.Add(TransactionFactory.Create(TransactionType.StakingOperation, FiatCurrency.Eur, singleEvent.Asset,
                    //    Math.Abs(singleEvent.Amount), 1m, singleEvent.Time));
                }
                else if (singleEvent.TypeAsString == "staking")
                {
                    // Staking single operation "finished"
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
                    // "","id2","2021-07-07 06:59:26","deposit","","currency","ZEUR",500.0000,0.0000,""
                    // "id1","id2","2021-07-07 07:00:00","deposit","","currency","ZEUR",500.0000,0.0000,521.2828
                    result.Add(TransactionFactory.Create(TransactionType.Deposit, FiatCurrency.Eur, "",
                        Math.Abs(item1.Amount), 1m, item1.Time));
                }
                else if (info.WithdrawalCount == 2)
                {
                    // "","id2","2021-01-12 14:40:43","withdrawal","","currency","ZEUR",-599.9100,0.0900,""
                    // "id1","id2","2021-01-12 14:43:49","withdrawal","","currency","ZEUR",-599.9100,0.0900,64.6800
                    result.Add(TransactionFactory.Create(TransactionType.Withdrawal, FiatCurrency.Eur, "",
                        Math.Abs(item1.Amount), 1m, item1.Time));
                }
                else if (info.IsTransfer)
                {
                    if (info.WithdrawalCount == 1)
                    {
                        result.Add(TransactionFactory.CreateStakingTransfer(info.StakeTransferType, true, item1, item2));
                    }
                    else if (info.DepositCount == 1)
                    {
                        // Withdrawal-side should be sufficient
                        //result.Add(TransactionFactory.CreateStakingTransfer(info.StakeTransferType, false, item1, item2));
                    }
                }
                else if (info.IsTrade)
                {
                    // "id1a","id2","2021-03-09 08:03:59","trade","","currency","ZEUR",-499.4582,1.2986,306.0206
                    // "id1b","id2","2021-03-09 08:03:59","trade","","currency","TRX",11286.68171558,0.00000000,11286.68171558
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
