using System.Globalization;
using System.Text;
using vergiFinance.Brokers.Kraken.Operations;

namespace vergiFinance.Brokers.Kraken;

/// <summary>
/// Parsed log that supports various operations
/// </summary>
class KrakenLog : IEventLog
{
    private const string Separator = "   ";
    public List<TransactionBase> Transactions { get; set; } = new List<TransactionBase>();

    private IEnumerable<TransactionBase> GetBuySellTransactions() => Transactions
        .Where(t => t.Type is TransactionType.Buy or TransactionType.Sell);

    public KrakenLog(List<RawTransaction> transactions)
    {
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
                Transactions.Add(TransactionFactory.Create(TransactionType.StakingOperation, FiatCurrency.Eur, singleEvent.Asset,
                    Math.Abs(singleEvent.Amount), 1m, singleEvent.Time));
            }
            else if (singleEvent.TypeAsString == "deposit")
            {
                Transactions.Add(TransactionFactory.Create(TransactionType.StakingDividend, FiatCurrency.Eur, singleEvent.Asset,
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
                Transactions.Add(TransactionFactory.Create(TransactionType.Deposit, FiatCurrency.Eur, "",
                    Math.Abs(item1.Amount), 1m, item1.Time));
            }
            else if (info.WithdrawalCount == 2)
            {
                Transactions.Add(TransactionFactory.Create(TransactionType.Withdrawal, FiatCurrency.Eur, "",
                    Math.Abs(item1.Amount), 1m, item1.Time));
            }
            else if (info.IsTransfer)
            {
                if (info.WithdrawalCount == 1)
                {
                    Transactions.Add(TransactionFactory.CreateStakingTransfer(TransactionType.StakingWithdrawal, item1, item2));
                }
                else if (info.DepositCount == 1)
                {
                    Transactions.Add(TransactionFactory.CreateStakingTransfer(TransactionType.StakingDeposit, item1, item2));
                }
            }
            else if (info.IsTrade)
            {
                Transactions.Add(TransactionFactory.CreateKrakenTrade(item1, item2));
            }
        }
    }

    private (List<RawTransaction> singles, List<(RawTransaction, RawTransaction)> pairs) CombineTransactions(
        List<RawTransaction> transactions)
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

        // <ticker, ticker transactions>
        var dict = new Dictionary<string, List<TransactionBase>>();
        foreach (var transaction in GetBuySellTransactions().Where(t => t.TradeDate.Year <= year))
        {
            if (dict.ContainsKey(transaction.Ticker))
            {
                dict[transaction.Ticker].Add(transaction);
            }
            else
            {
                dict.Add(transaction.Ticker, new List<TransactionBase>() { transaction });
            }
        }

        // <ticker, transactions report>
        var transactionReport = new Dictionary<string, string>();

        foreach (var entry in dict)
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

        foreach (var ticker in dict.Keys)
        {
            messageBuilder.AppendLine("--------------------");
            messageBuilder.AppendLine($"Ticker: {ticker}");

            //
            messageBuilder.Append(transactionReport[ticker]);

            //
            messageBuilder.AppendLine($"Sales profit report");
            var salesCalculator = new SalesCalculator(dict[ticker], year);
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

    public string PrintStakingReport(int year)
    {
        var stakingRewards = Transactions.Where(t => t.Type == TransactionType.StakingDividend).ToList();
        var priceFetcher = new PriceFetcher();
        priceFetcher.FillDayUnitPrice(stakingRewards).Wait();

        // Each transaction object now filled with proper day unit price
        priceFetcher.SaveUnitPrices(stakingRewards);

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
        var years = GetBuySellTransactions().Select(t => t.TradeDate.Year).Distinct().ToList();
        return years.OrderBy(y => y).ToList();
    }
}