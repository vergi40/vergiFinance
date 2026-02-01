using System.Text;
using System.Transactions;
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
                    // Duplicate event to "staking" reward event. Can be skipped, if rules apply:
                    // * Doesn't contain txid
                    // * Type = "deposit"
                    // * (optional) same amount as matching "staking" event

                    // "","FTCSzfC-N9GEQc5BIU6rNED697D59N","2023-12-17 01:32:55","deposit","","currency","XETH",0.0001868805,0.0000280321,""
                    // "LJVYMU-G7PLP-YQEUIG","ST7U3YD-5ZDUV-QKJZRH","2023-12-17 02:11:03","staking","","currency","XETH",0.0001868805,0.0000280321,0.9774000144
                    continue;
                }
                else if (singleEvent.TypeAsString == "staking")
                {
                    // TODO skip now
                    continue;
                    // Staking single operation "finished"
                    // "staking"-side event also contains fee that is subtracted before reward is added to wallet
                    result.Add(TransactionFactory.CreateWithFee(TransactionType.StakingDividend, FiatCurrency.Eur, singleEvent.Asset,
                        Math.Abs(singleEvent.Amount), 1m, singleEvent.Time, singleEvent.Fee));
                }
                else if (singleEvent.TypeAsString == "withdrawal")
                {
                    result.Add(TransactionFactory.Create(TransactionType.Withdrawal, FiatCurrency.Eur, "",
                        Math.Abs(singleEvent.Amount), 1m, singleEvent.Time));
                }
                else if (singleEvent.TypeAsString == "transfer")
                {
                    // TODO skip now
                    continue;
                    if (singleEvent.SubType == "spottostaking" || singleEvent.SubType == "stakingfromspot")
                    {
                        result.Add(TransactionFactory.CreateStakingTransfer(TransactionType.WalletToStaking, true, 
                            singleEvent, singleEvent));
                    }
                    else if (singleEvent.SubType == "stakingtospot" || singleEvent.SubType == "spotfromstaking")
                    {
                        result.Add(TransactionFactory.CreateStakingTransfer(TransactionType.StakingToWallet, true, 
                            singleEvent, singleEvent));
                    }
                    else
                    {
                        throw new NotImplementedException($"Type [{singleEvent.TypeAsString}] transaction not implemented");
                    }
                }
                else if (singleEvent.TypeAsString == "earn")
                {
                    // TODO skip now
                    continue;
                }
                else
                {
                    throw new NotImplementedException($"Type [{singleEvent.TypeAsString}] transaction not implemented");
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
                        // TODO skip now
                        continue;
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
                // TODO else. Earn etc
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

            var multis = dict.Values.Where(v => v.Count > 2).ToList();
            var multisAsPairs = CombineMultiTransactions(multis);
            pairs.AddRange(multisAsPairs);

            return (singles, pairs.Select(p => (p[0], p[1])).ToList());
        }

        private List<List<RawTransaction>> CombineMultiTransactions(List<List<RawTransaction>> multis)
        {
            // 2025
            // "txid","refid","time","type","subtype","aclass","subclass","asset","wallet","amount","fee","balance"
            // "tid1","rid1","2025-07-12 09:55:49","earn","autoallocation","currency","crypto","ADA","spot / main",-0.02186300,0,0.00000000
            // "tid2","rid1","2025-07-12 09:55:49","earn","autoallocation","currency","crypto","ADA","earn / liquid",0.02186300,0,0.02186300
            // "tid3","rid1","2025-07-12 09:55:49","earn","autoallocation","currency","crypto","XTZ","spot / main",-0.01464600,0,0.00000000
            // "tid4","rid1","2025-07-12 09:55:49","earn","autoallocation","currency","crypto","XTZ","earn / liquid",0.01464600,0,0.01464600
            var pairs = new List<List<RawTransaction>>();

            foreach (var multi in multis)
            {
                // All with same refid
                var assetTypes = multi.Select(m => m.Asset).Distinct();

                foreach (var asset in assetTypes)
                {
                    var debug = new StringBuilder();
                    debug.AppendLine($"Reference id: {multi[0].ReferenceId}, asset: {asset}");
                    var newPair = multi.Where(m => m.Asset == asset).ToList();

                    if (newPair.Count != 2)
                    {
                        foreach (var item in newPair)
                        {
                            debug.AppendLine($"  Event: {item}");
                        }
                        throw new NotImplementedException($"Unrecognized multi-transaction with same refid - have more than 2 events with same asset: \n" +
                                                          $"{debug}");
                    }

                    pairs.Add(newPair);
                }
            }

            return pairs;
        }
    }
}
