﻿using vergiFinance.Brokers.Kraken.Operations;
using vergiFinance.Model;

namespace vergiFinance.FinanceFunctions
{
    public static class SalesFactory
    {
        /// <summary>
        /// Create sales calculator and process transactions for a year.
        /// After this the result can be used to fetch profits and prints
        /// </summary>
        public static (ISalesResult, IHoldingsResult) ProcessSalesForYear(List<TransactionBase> allTransactionsForTicker, int year)
        {
            var calculator = new SalesCalculator(year);
            var sales = calculator.CalculateCumulativeSales(allTransactionsForTicker);
            return sales;
        }

        /// <summary>
        /// Create sales calculator and process transactions and staking transfers for a year.
        /// </summary>
        public static (ISalesResult, IHoldingsResult) ProcessSalesAndStakingForYear(List<TransactionBase> allTransactionsForTicker, int year, IPriceFetcher fetcher)
        {
            var calculator = new SalesCalculator(year, fetcher);
            var sales = calculator.CalculateCumulativeSalesWithStaking(allTransactionsForTicker);
            return sales;
        }
    }

    internal class SalesCalculator : ISalesCalculator
    {
        private List<SalesUnitInformation> _allSales { get; }
        private int _year { get; }
        private IPriceFetcher? _fetcher { get; }

        public string Ticker { get; private set; } = "null";

        /// <summary>
        /// Tax implications
        /// </summary>
        public bool ContainsSellEvents { get; private set; }
        /// <summary>
        /// Tax implications
        /// </summary>
        public bool ContainsStakingWithdrawals { get; private set; }

        public StakingInfo Staking { get; } = new();
        public IHoldingsResult? Holdings { get; private set; } = null;

        public SalesCalculator(int year)
        {
            _year = year;
            _allSales = new();
            _fetcher = null;
        }

        public SalesCalculator(int year, IPriceFetcher fetcher)
        {
            _year = year;
            _allSales = new();
            _fetcher = fetcher;
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

        private IEnumerable<TransactionBase> GetBuySellTransactions(
            IEnumerable<TransactionBase> transactions)
        {
            return transactions
                .Where(t => t.Type is
                    TransactionType.Buy or TransactionType.Sell);
        }

        /// <summary>
        /// Prerequisite: All transactions are same ticker. Types are filtered
        /// </summary>
        /// <exception cref="ArgumentException">Logical error in transaction content.</exception>
        public (ISalesResult, IHoldingsResult) CalculateCumulativeSalesWithStaking(List<TransactionBase> transactions)
        { 
            if (_fetcher == null) throw new InvalidOperationException("Logical error");
            _allSales.Clear();
            if (transactions.Any()) Ticker = transactions[0].Ticker;


            var totalProfit = 0m;
            var buyHistoryFifo = new Queue<TransactionBase>();

            var accounts = new KrakenAccounts();
            var avPrice = new AveragePrice();

            foreach (var transaction in GetTransactionsForTaxCalculations(transactions))
            {
                if (transaction.Type == TransactionType.StakingDividend)
                {
                    accounts.StakingDividends.Add(transaction.AssetAmount);
                    Staking.AddDividend(transaction);
                }
                else if (transaction.Type == TransactionType.WalletToStaking)
                {
                    var amount = transaction.AssetAmount;
                    accounts.Crypto.Subtract(amount);
                    accounts.Staking.Add(amount);
                }
                else if (transaction.Type == TransactionType.StakingToWallet)
                {
                    // NOTE: Accounting strategy is not FIFO now, but:
                    // * First withdraw already owned, staked
                    // * Lastly withdraw staking dividends

                    var withdrawalAmount = transaction.AssetAmount;

                    // Withdraw just a portion of invested, or invested + dividends?
                    if (withdrawalAmount < accounts.Staking.Amount)
                    {
                        // Return part of the staked crypto to wallet
                        accounts.Staking.Subtract(withdrawalAmount);
                        accounts.Crypto.Add(withdrawalAmount);
                    }
                    else
                    {
                        // Returning dividends also
                        var originalStakedRemaining = accounts.Staking.Amount;
                        var dividendsAmount = withdrawalAmount - originalStakedRemaining;

                        // Stake wallet goes to zero
                        accounts.Staking.Subtract(originalStakedRemaining);
                        accounts.Crypto.Add(originalStakedRemaining);
                        Staking.AddWithdrawal(transaction, transaction.AssetUnitPrice, originalStakedRemaining);

                        // "cash out" the dividends
                        accounts.Crypto.Add(dividendsAmount);
                        accounts.StakingDividends.Subtract(dividendsAmount);

                        // Calculate price for the dividends
                        // TODO to async
                        var dividendUnitPriceNullable = Task.Run(() =>
                            _fetcher.GetCoinPriceForDate(transaction.Ticker, transaction.TradeDate)).GetAwaiter().GetResult();
                        if (dividendUnitPriceNullable == null)
                        {
                            throw new InvalidOperationException(
                                $"Didn't receive valid coin price for {transaction.Ticker} " +
                                $"at date {transaction.TradeDate}");
                        }

                        var dividendUnitPrice = dividendUnitPriceNullable.Value;

                        // Creating "fake" buy event. Staking rewards are appreciated based on current rate
                        buyHistoryFifo.Enqueue(TransactionFactory.CreateBuy(FiatCurrency.Eur, transaction.Ticker,
                            dividendsAmount, dividendUnitPrice, transaction.TradeDate));
                        avPrice.AddDividendEvent(dividendsAmount, dividendsAmount * dividendUnitPrice);

                        var sale = new SalesUnitInformation(dividendUnitPrice, 0, dividendsAmount, transaction);
                        _allSales.Add(sale);


                        // TODO not sure, now setting only if withdrawing dividends
                        ContainsStakingWithdrawals = true;
                    }
                }
                else if (transaction.Type == TransactionType.Buy)
                {
                    accounts.Fiat.Subtract(transaction.TotalPrice);
                    avPrice.AddBuyEvent(transaction.AssetAmount, transaction.TotalPrice);
                    accounts.Crypto.Add(transaction.AssetAmount);
                    
                    buyHistoryFifo.Enqueue(transaction.DeepCopy());
                }
                else if (transaction.Type == TransactionType.Sell)
                {
                    accounts.Fiat.Add(transaction.TotalPrice);
                    avPrice.AddSellEvent(transaction.AssetAmount);
                    accounts.Crypto.Subtract(transaction.AssetAmount);
                    ContainsSellEvents = true;
                    
                    // Sold less than there exists in first buy
                    if (transaction.AssetAmount <= buyHistoryFifo.First().AssetAmount)
                    {
                        var buy = buyHistoryFifo.First();
                        buy.AssetAmount -= transaction.AssetAmount;
                        var profit = (transaction.AssetUnitPrice - buy.AssetUnitPrice) * transaction.AssetAmount;

                        // 
                        var sale = new SalesUnitInformation(transaction.AssetUnitPrice, buy.AssetUnitPrice, transaction.AssetAmount, transaction);
                        _allSales.Add(sale);

                        totalProfit += profit;
                        if (Math.Abs(buy.AssetAmount) < 1e-6m) buyHistoryFifo.Dequeue();
                    }
                    // Iterate buy list until asset amount is subtracted
                    else
                    {
                        var assetAmount = transaction.AssetAmount;
                        var counter = 0;
                        while (Math.Abs(assetAmount) >= 1e-6m)
                        {
                            if (!buyHistoryFifo.Any())
                            {
                                throw new ArgumentException($"Failed to calculate: there are more sell-units than buy-units. " +
                                                            $"Current transaction in iteration: {transaction}");
                            }

                            var buy = buyHistoryFifo.First();
                            var maxReduce = Math.Min(Math.Abs(buy.AssetAmount), assetAmount);

                            assetAmount -= maxReduce;
                            buy.AssetAmount -= maxReduce;
                            var profit = (transaction.AssetUnitPrice - buy.AssetUnitPrice) * maxReduce;

                            // 
                            var sale = new SalesUnitInformation(transaction.AssetUnitPrice, buy.AssetUnitPrice, maxReduce, transaction);
                            _allSales.Add(sale);

                            totalProfit += profit;
                            if (Math.Abs(buy.AssetAmount) < 1e-6m) buyHistoryFifo.Dequeue();

                            counter++;
                            if (counter > 50)
                            {
                                throw new ArgumentException($"Failed to calculate, invalid data (iteration counter reached: {counter}). " +
                                                            $"Current transaction in iteration: {transaction}");
                            }
                        }
                    }
                }
            }

            //if (stakeWithdrawalsEur.Amount > 0)
            //{
            //    // Assert that wallets match. totalProfit doesn't include staking income
            //    var fiatProfit = fiat.Amount;
            //    var debug = stakeWithdrawalsEur.Amount + totalProfit - fiatProfit;
            //    //if (Math.Abs(stakeWithdrawalsEur.Amount + totalProfit - fiatProfit) > 0.1m)
            //    //{
            //    //    throw new InvalidOperationException("Data error");
            //    //}
            //}

            var walletAveragePrice = avPrice.GetAveragePrice();

            Holdings = new HoldingsResult(Ticker, 
                accounts.Crypto.Amount, 
                accounts.Staking.Amount + accounts.StakingDividends.Amount, 
                walletAveragePrice);
            return (new SalesResult(_allSales, _year, Staking), Holdings);
        }

        /// <summary>
        /// Prerequisite: All transactions are same ticker. Only buy or sell types
        /// </summary>
        /// <exception cref="ArgumentException">Logical error in transaction content.</exception>
        public (ISalesResult, IHoldingsResult) CalculateCumulativeSales(List<TransactionBase> transactions)
        {
            _allSales.Clear();
            if (transactions.Any()) Ticker = transactions[0].Ticker;
            // Examples
            // Buy 100 @5, 500
            // Buy 100 @10, 1000
            // Sell 50 @10, 500. FIFO first shares (50) rose 250 -> 500 = 250 profit.
            // Sell 100 @20, 2000. FIFO (50) 250 -> 1000, (50) 500 -> 1000. = 1250 profit.
            // 50 shares left

            var totalProfit = 0m;
            var buyHistory = new List<TransactionBase>();
            var cryptoWallet = new Account();
            foreach (var transaction in GetBuySellTransactions(transactions))
            {
                if (transaction.Type == TransactionType.Buy)
                {
                    cryptoWallet.Add(transaction.AssetAmount);
                    buyHistory.Add(transaction.DeepCopy());
                }
                else if (transaction.Type == TransactionType.Sell)
                {
                    cryptoWallet.Subtract(transaction.AssetAmount);
                    // Sold less than there exists in first buy
                    if (transaction.AssetAmount <= buyHistory.First().AssetAmount)
                    {
                        var buy = buyHistory.First();
                        buy.AssetAmount -= transaction.AssetAmount;
                        var profit = (transaction.AssetUnitPrice - buy.AssetUnitPrice) * transaction.AssetAmount;

                        // 
                        var sale = new SalesUnitInformation(transaction.AssetUnitPrice, buy.AssetUnitPrice, transaction.AssetAmount, transaction);
                        _allSales.Add(sale);

                        totalProfit += profit;
                        if (Math.Abs(buy.AssetAmount) < 1e-6m) buyHistory.RemoveAt(0);
                    }
                    // Iterate buy list until asset amount is subtracted
                    else
                    {
                        var assetAmount = transaction.AssetAmount;
                        var counter = 0;
                        while (Math.Abs(assetAmount) >= 1e-6m)
                        {
                            if (!buyHistory.Any())
                            {
                                throw new ArgumentException($"Failed to calculate: there are more sell-units than buy-units. " +
                                                            $"Current transaction in iteration: {transaction}");
                            }

                            var buy = buyHistory.First();
                            var maxReduce = Math.Min(Math.Abs(buy.AssetAmount), assetAmount);

                            assetAmount -= maxReduce;
                            buy.AssetAmount -= maxReduce;
                            var profit = (transaction.AssetUnitPrice - buy.AssetUnitPrice) * maxReduce;

                            // 
                            var sale = new SalesUnitInformation(transaction.AssetUnitPrice, buy.AssetUnitPrice, maxReduce, transaction);
                            _allSales.Add(sale);

                            totalProfit += profit;
                            if (Math.Abs(buy.AssetAmount) < 1e-6m) buyHistory.RemoveAt(0);

                            counter++;
                            if (counter > 50)
                            {
                                throw new ArgumentException($"Failed to calculate, invalid data (iteration counter reached: {counter}). " +
                                                            $"Current transaction in iteration: {transaction}");
                            }
                        }
                    }
                }
            }

            Holdings = new HoldingsResult(Ticker, cryptoWallet.Amount);
            return (new SalesResult(_allSales, _year), Holdings);
        }
    }

    public record TotalPurchasesAndSales(decimal lossPurchases, decimal lossSales, decimal profitPurchases,
        decimal profitSales);

    public enum SalesType { Profit, Loss, StakingWithdrawal }

    /// <summary>
    /// Contains full or partial transaction focusing on selling unit price
    /// </summary>
    internal class SalesUnitInformation
    {
        public SalesType Type { get; set; }
        public TransactionBase? _originalFullTransaction { get; set; }

        /// <summary>
        /// When trade occured
        /// </summary>
        public DateTime TradeDate { get; set; }

        /// <summary>
        /// Amount of units in the transaction. Non-negative.
        /// Kpl määrä. Esim 8 kpl osakkeita. 0.001 kpl kryptoa
        /// </summary>
        public decimal AssetAmount { get; set; }

        /// <summary>
        /// Price of a singe unit (single share, single coin) in fiat currency. Non-negative.
        /// Kurssi, osakekurssi
        /// </summary>
        public decimal SoldUnitPrice { get; set; }
        /// <summary>
        /// Price of a singe unit (single share, single coin) in fiat currency. Non-negative.
        /// Kurssi, osakekurssi
        /// </summary>
        public decimal OriginalUnitPrice { get; set; }

        /// <summary>
        /// Transaction total price in fiat currency. Non-negative.
        /// Kauppahinta
        /// </summary>
        public decimal SoldTotalPrice => AssetAmount * SoldUnitPrice;
        public decimal BoughtTotalPrice => AssetAmount * OriginalUnitPrice;

        public decimal ProfitLoss { get; set; }

        public SalesUnitInformation(decimal soldUnitPrice, decimal originalUnitPrice, decimal assetAmount, TransactionBase originalFullTransaction)
        {
            _originalFullTransaction = originalFullTransaction;
            if (_originalFullTransaction.Type == TransactionType.StakingToWallet)
            {
                Type = SalesType.StakingWithdrawal;
            }
            else if (soldUnitPrice >= originalUnitPrice)
            {
                Type = SalesType.Profit;
            }
            else
            {
                Type = SalesType.Loss;
            }

            AssetAmount = assetAmount;
            SoldUnitPrice = soldUnitPrice;
            OriginalUnitPrice = originalUnitPrice;
            ProfitLoss = (soldUnitPrice - originalUnitPrice) * assetAmount;
            TradeDate = originalFullTransaction.TradeDate;
        }
    }
}
