using System.Transactions;
using vergiFinance.Model;

namespace vergiFinance.Brokers.Kraken
{
    internal class TickerOrganizer
    {
        public Dictionary<string, List<TransactionBase>> AllByTicker { get; } = new();

        /// <summary>
        /// E.g. TRX-named ticker will contain all TRX.S events. No TRX.S entry in this case
        /// </summary>
        public Dictionary<string, List<TransactionBase>> AllByTickerWithStaking { get; } = new();
        public Dictionary<string, List<TransactionBase>> BuySellByTicker { get; } = new();

        public TickerOrganizer(IReadOnlyList<TransactionBase> transactions, int year)
        {
            foreach (var transaction in transactions.Where(t => t.TradeDate.Year <= year))
            {
                if (AllByTicker.ContainsKey(transaction.Ticker))
                {
                    AllByTicker[transaction.Ticker].Add(transaction);
                }
                else
                {
                    AllByTicker.Add(transaction.Ticker, new List<TransactionBase>() { transaction });
                }
            }

            foreach (var transaction in GetBuySellTransactions(transactions).Where(t => t.TradeDate.Year <= year))
            {
                if (BuySellByTicker.ContainsKey(transaction.Ticker))
                {
                    BuySellByTicker[transaction.Ticker].Add(transaction);
                }
                else
                {
                    BuySellByTicker.Add(transaction.Ticker, new List<TransactionBase>() { transaction });
                }
            }

            foreach (var transaction in transactions.Where(t => t.TradeDate.Year <= year))
            {
                var ticker = transaction.Ticker;
                if (ticker.EndsWith(".S"))
                {
                    // TODO ETH2 and other oddities handling
                    ticker = ticker.Substring(0, ticker.Length - 2);
                }
                if (AllByTickerWithStaking.ContainsKey(ticker))
                {
                    AllByTickerWithStaking[ticker].Add(transaction);
                }
                else
                {
                    AllByTickerWithStaking.Add(ticker, new List<TransactionBase>() { transaction });
                }
            }
        }

        public TickerOrganizer(IReadOnlyList<TransactionBase> transactions, DateTime date)
        {
            foreach (var transaction in transactions.Where(t => t.TradeDate <= date))
            {
                if (AllByTicker.ContainsKey(transaction.Ticker))
                {
                    AllByTicker[transaction.Ticker].Add(transaction);
                }
                else
                {
                    AllByTicker.Add(transaction.Ticker, new List<TransactionBase>() { transaction });
                }
            }

            foreach (var transaction in GetBuySellTransactions(transactions).Where(t => t.TradeDate <= date))
            {
                if (BuySellByTicker.ContainsKey(transaction.Ticker))
                {
                    BuySellByTicker[transaction.Ticker].Add(transaction);
                }
                else
                {
                    BuySellByTicker.Add(transaction.Ticker, new List<TransactionBase>() { transaction });
                }
            }

            foreach (var transaction in transactions.Where(t => t.TradeDate <= date))
            {
                var ticker = transaction.Ticker;
                if (ticker.EndsWith(".S"))
                {
                    // TODO ETH2 and other oddities handling
                    ticker = ticker.Substring(0, ticker.Length - 2);
                }
                if (AllByTickerWithStaking.ContainsKey(ticker))
                {
                    AllByTickerWithStaking[ticker].Add(transaction);
                }
                else
                {
                    AllByTickerWithStaking.Add(ticker, new List<TransactionBase>() { transaction });
                }
            }
        }

        private IEnumerable<TransactionBase> GetBuySellTransactions(
            IEnumerable<TransactionBase> transactions)
        {
            return transactions
                .Where(t => t.Type is
                    TransactionType.Buy or TransactionType.Sell);
        }

        /// <summary>
        /// Buy, sell, to staking, from staking
        /// </summary>
        private IEnumerable<TransactionBase> GetTransactionsForTaxCalculations(
            IEnumerable<TransactionBase> transactions)
        {
            return transactions
                .Where(t => t.Type is
                    TransactionType.Buy or TransactionType.Sell or
                    TransactionType.WalletToStaking or TransactionType.StakingToWallet or
                    TransactionType.StakingDividend);
        }
    }
}
