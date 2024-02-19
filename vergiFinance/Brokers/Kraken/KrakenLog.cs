using System.Globalization;
using System.Text;
using vergiFinance.Brokers.Kraken.Operations;
using vergiFinance.FinanceFunctions;
using vergiFinance.Model;

namespace vergiFinance.Brokers.Kraken;

static class EventLogFactory
{
    public static IEventLog CreateKrakenLog(IReadOnlyList<RawTransaction> transactions)
    {
        var eventLog = new KrakenLog();
        eventLog.ProcessRawTransactions(transactions);
        return eventLog;
    }
}

/// <summary>
/// Parsed log that supports various operations
/// </summary>
class KrakenLog : IEventLog
{
    private const string Separator = "   ";

    /// <summary>
    /// All transactions and types
    /// </summary>
    public List<TransactionBase> Transactions { get; set; } = new List<TransactionBase>();

    public void ProcessRawTransactions(IReadOnlyList<RawTransaction> transactions)
    {
        var processer = new RawTransactionProcesser();
        Transactions = processer.ProcessRawTransactions(transactions);

        Sort();
    }

    public (ISalesResult, IHoldingsResult) CalculateSales(int year, string ticker, IPriceFetcher fetcher)
    {
        var data = new TickerOrganizer(Transactions, year);

        var transactions = data.AllByTickerWithStaking[ticker].Where(t => t.TradeDate.Year <= year).ToList();
        var sales = SalesFactory.ProcessSalesAndStakingForYear(transactions, year, fetcher);
        return sales;
    }

    /// <summary>
    /// Take all transactions from start to target year. Calculate profit/loss for each sell-event.
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    public string PrintExtendedTaxReport(int year)
    {
        var fiat = 'e';
        //var culture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fi-FI");
        var fetcher = new PriceFetcherWithPersistence();

        var data = new TickerOrganizer(Transactions, year);
        // <ticker, ticker transactions>
        var dictAll = data.AllByTickerWithStaking;

        // <ticker, transactions report>
        var transactionReport = new Dictionary<string, string>();

        foreach (var entry in dictAll)
        {
            var eventsBuilder = new StringBuilder();
            eventsBuilder.AppendLine($"Transactions summary");
            var sorted = entry.Value.ToList();
            sorted = sorted.OrderBy(t => t.TradeDate).ToList();

            foreach (var transaction in sorted)
            {
                eventsBuilder.AppendLine($"   {transaction.ToKrakenTransactionString(fiat)}");
            }

            eventsBuilder.AppendLine();
            transactionReport.Add(entry.Key, eventsBuilder.ToString());
        }

        var messageBuilder = new StringBuilder();

        var totalProfit = 0m;
        var totalLoss = 0m;
        var allTotals = new List<TotalPurchasesAndSales>();
        messageBuilder.AppendLine();
        messageBuilder.AppendLine($"Tax report for sales in {year}");
        messageBuilder.AppendLine("Individual listing for each ticker:");

        foreach (var ticker in dictAll.Keys)
        {
            messageBuilder.AppendLine("--------------------");
            messageBuilder.AppendLine($"Ticker: {ticker}");

            // Transactions summary
            messageBuilder.Append(transactionReport[ticker]);

            var (salesCalculator, holding) = SalesFactory.ProcessSalesAndStakingForYear(dictAll[ticker], year, fetcher);

            // Holdings
            if (holding.AssetAmountTotal > 0.001m)
            {
                messageBuilder.AppendLine("Current holdings (WIP not entirely correct with all transaction types):");
                messageBuilder.AppendLine($"{Separator}Wallet: {holding.AssetAmountInWallet}");
                messageBuilder.AppendLine($"{Separator}Staked: {holding.AssetAmountStaked}");
                messageBuilder.AppendLine();
            }

            // Staking summary
            if (salesCalculator.Staking.HasEvents)
            {
                messageBuilder.AppendLine($"Staking recap");
                messageBuilder.AppendLine($"{Separator}Staking rewards all time total: {salesCalculator.Staking.TotalDividends()}");
                messageBuilder.AppendLine($"{Separator}Staking rewards moved to spot all time (taxable): {salesCalculator.Staking.TotalWithdrawals()}");
                messageBuilder.AppendLine($"{Separator}Amount still in staking ({year}): WIP");
                messageBuilder.AppendLine();
            }

            //Sales profit report
            messageBuilder.AppendLine($"Sales profit report");

            var profitSales = salesCalculator.PrintProfitSales().ToList();
            if (profitSales.Any())
            {
                foreach (var sale in profitSales)
                {
                    messageBuilder.AppendLine($"{Separator}{sale}");
                }

                var profit = salesCalculator.TotalProfit();
                totalProfit += profit;
                messageBuilder.AppendLine($"{Separator}Total profit sales: {profit:F2}{fiat}");
                messageBuilder.AppendLine();
            }

            // Sales loss report
            messageBuilder.AppendLine($"Sales loss report");
            var lossSales = salesCalculator.PrintLossSales().ToList();
            if (lossSales.Any())
            {
                foreach (var sale in lossSales)
                {
                    messageBuilder.AppendLine($"{Separator}{sale}");
                }

                var loss = salesCalculator.TotalLoss();
                totalLoss += loss;
                messageBuilder.AppendLine($"{Separator}Total loss sales: {loss:F2}{fiat}");
                messageBuilder.AppendLine();
            }

            allTotals.Add(salesCalculator.CalculateTotalPurchasesAndSales());
            var tickerTotal = salesCalculator.TotalProfitLoss();
            messageBuilder.AppendLine($"Sales total profit/loss: {tickerTotal:F2}{fiat}");
            messageBuilder.AppendLine();
        }

        messageBuilder.AppendLine("--------------------");
        messageBuilder.AppendLine($"Profit summary (Luovutuksen tiedot & Hankintatiedot ja kulut)");
        messageBuilder.AppendLine($"{Separator}Total purchase price for cryptos sold with profit: " +
                                  $"{allTotals.Sum(t => t.profitPurchases):F2}{fiat}");
        messageBuilder.AppendLine($"{Separator}Total sales price for cryptos sold {year} with profit: " +
                                  $"{allTotals.Sum(t => t.profitSales):F2}{fiat}");
        messageBuilder.AppendLine();

        messageBuilder.AppendLine($"Loss summary (Luovutuksen tiedot & Hankintatiedot ja kulut)");
        messageBuilder.AppendLine($"{Separator}Total purchase price for cryptos sold with loss: " +
                                  $"{allTotals.Sum(t => t.lossPurchases):F2}{fiat}");
        messageBuilder.AppendLine($"{Separator}Total sales price for cryptos sold {year} with loss: " +
                                  $"{allTotals.Sum(t => t.lossSales):F2}{fiat}");
        messageBuilder.AppendLine();

        messageBuilder.AppendLine("--------------------");
        messageBuilder.AppendLine();
        messageBuilder.AppendLine($"Total profit for year {year}:      {totalProfit:F2}{fiat}");
        messageBuilder.AppendLine($"Total loss for year {year}:        {totalLoss:F2}{fiat}");
        messageBuilder.AppendLine($"Total profit/loss for year {year}: {totalProfit+totalLoss:F2}{fiat}");
        messageBuilder.AppendLine("--------------------");
        messageBuilder.AppendLine();

        return messageBuilder.ToString();
    }

    public void Sort()
    {
        Transactions.Sort((a, b) => a.TradeDate.CompareTo(b.TradeDate));
    }

    public string PrintStakingReport(int year)
    {
        var stakingRewards = Transactions.Where(t => t.Type == TransactionType.StakingDividend).ToList();
        var priceFetcher = new PriceFetcher();
        priceFetcher.FillDayUnitPrice(stakingRewards).Wait();

        // Each transaction object now filled with proper day unit price
        //priceFetcher.SaveUnitPrices(stakingRewards);

        var message = new StringBuilder();
        message.AppendLine($"Staking events for year {year}:");
        foreach (var reward in stakingRewards)
        {
            message.AppendLine($"  {reward.ToKrakenDividendString()}");
        }

        message.AppendLine("-----");
        message.AppendLine($"Total staking rewards: {stakingRewards.Sum(r => r.DayUnitPrice * r.AssetAmount):F2}e");
        message.AppendLine("-----");

        return message.ToString();
    }

    public List<int> TransactionYearSpan()
    {
        var years = Transactions.Select(t => t.TradeDate.Year).Distinct().ToList();
        return years.OrderBy(y => y).ToList();
    }

    public IAllHoldingsResult CalculateAllHoldings(DateTime pointInTime)
    {
        var dataAll = new TickerOrganizer(Transactions, pointInTime);
        var dictAll = dataAll.AllByTickerWithStaking;

        var holdings = new List<IHoldingsResult>();
        foreach (var key in dictAll.Keys)
        {
            var holding = CalculateHoldings(pointInTime, key);
            holdings.Add(holding);
        }

        return new AllHoldingsResult(holdings);
    }

    public IHoldingsResult CalculateHoldings(DateTime pointInTime, string ticker)
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fi-FI");

        var dataAll = new TickerOrganizer(Transactions, pointInTime);
        var dictAll = dataAll.AllByTicker;

        var calculator = new HoldingsCalculator();

        // TODO more weird holdings like ETH
        if (dictAll.ContainsKey($"{ticker}.S"))
        {
            var transactions = dictAll[ticker]
                .Concat(dictAll[$"{ticker}.S"])
                .OrderBy(t => t.TradeDate)
                .ToList();
            return calculator.CalculateHoldingsWithStaking( ticker, transactions);
        }
        return calculator.CalculateHoldings(ticker, dictAll[ticker]);
    }

    
}