using vergiFinance.Model;

namespace vergiFinance.FinanceFunctions
{
    public static class SalesFactory
    {
        /// <summary>
        /// Create sales calculator and process transactions for a year.
        /// After this the calculator can be used to fetch profits and prints
        /// </summary>
        /// <param name="allTransactionsForTicker"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public static ISalesCalculator ProcessSalesForYear(List<TransactionBase> allTransactionsForTicker, int year)
        {
            var sales = new SalesCalculator(year, SalesPrinterFactory.CreateEng());
            sales.CalculateCumulativeSales(allTransactionsForTicker);
            return sales;
        }
    }

    internal class SalesCalculator : ISalesCalculator
    {
        private List<SalesUnitInformation> _allSales { get; }
        private int _year { get; }
        private SalesPrinter _printer { get; }
        
        public SalesCalculator(int year, SalesPrinter salesPrinter)
        {
            _year = year;
            _allSales = new();
            _printer = salesPrinter;
        }

        /// <summary>
        /// Prerequisite: All transactions are same ticker. Only buy or sell types
        /// </summary>
        /// <exception cref="ArgumentException">Logical error in transaction content.</exception>
        public void CalculateCumulativeSales(List<TransactionBase> transactions)
        {
            _allSales.Clear();
            // Examples
            // Buy 100 @5, 500
            // Buy 100 @10, 1000
            // Sell 50 @10, 500. FIFO first shares (50) rose 250 -> 500 = 250 profit.
            // Sell 100 @20, 2000. FIFO (50) 250 -> 1000, (50) 500 -> 1000. = 1250 profit.
            // 50 shares left

            var totalProfit = 0m;
            var buyHistory = new List<TransactionBase>();
            foreach (var transaction in transactions)
            {
                if (transaction.Type == TransactionType.Buy)
                {
                    buyHistory.Add(transaction.DeepCopy());
                }
                else if (transaction.Type == TransactionType.Sell)
                {
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
        }

        public IEnumerable<string> PrintProfitSales()
        {
            var profitSales = _allSales.Where(s => s.Type == SalesType.Profit && s.TradeDate.Year == _year);
            return _printer.PrintProfitSales(profitSales);
        }

        public IEnumerable<string> PrintLossSales()
        {
            var lossSales = _allSales.Where(s => s.Type == SalesType.Loss && s.TradeDate.Year == _year);
            return _printer.PrintLossSales(lossSales);
        }
        
        public decimal TotalProfit()
        {
            return _allSales.Where(s => s.Type == SalesType.Profit && s.TradeDate.Year == _year).Sum(s => s.ProfitLoss);
        }

        public decimal TotalLoss()
        {
            return _allSales.Where(s => s.Type == SalesType.Loss && s.TradeDate.Year == _year).Sum(s => s.ProfitLoss);
        }

        public decimal TotalProfitLoss()
        {
            return _allSales.Where(s => s.TradeDate.Year == _year).Sum(s => s.ProfitLoss);
        }

        public TotalPurchasesAndSales CalculateTotalPurchasesAndSales()
        {
            var lossList = _allSales.Where(s => s.Type == SalesType.Loss).ToList();
            var lossPurchases = lossList.Sum(s => s.BoughtTotalPrice);
            var lossSales = lossList.Sum(s => s.SoldTotalPrice);

            var profitList = _allSales.Where(s => s.Type == SalesType.Profit).ToList();
            var profitPurchases = profitList.Sum(s => s.BoughtTotalPrice);
            var profitSales = profitList.Sum(s => s.SoldTotalPrice);

            return new TotalPurchasesAndSales(lossPurchases, lossSales, profitPurchases, profitSales);
        }
    }

    public record TotalPurchasesAndSales(decimal lossPurchases, decimal lossSales, decimal profitPurchases,
        decimal profitSales);

    public enum SalesType { Profit, Loss }

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
            if (soldUnitPrice >= originalUnitPrice)
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
