using System.Collections.Generic;
using vergiFinance.Brokers.Kraken.Operations;

namespace vergiFinance
{
    // OPERATORS

    /// <summary>
    /// Broker that was used for transactions, e.g. Nordnet, Kraken ...
    /// </summary>
    public interface IBrokerService
    {
        public IEventLog ReadTransactions(IReadOnlyList<string> lines);
    }

    /// <summary>
    /// Collection entity that contains all strong-typed transactions and manipulation methods
    /// </summary>
    public interface IEventLog
    {
        List<TransactionBase> Transactions { get; set; }

        /// <summary>
        /// List all transactions. Print all sale events for given year. Give
        /// details individually for sales with profit and loss.
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        string PrintExtendedTaxReport(int year);

        /// <summary>
        /// List all staking events. Fetch current value for each stake reward
        /// and print total profit.
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        string PrintStakingReport(int year);

        List<int> TransactionYearSpan();
    }

    public interface ISalesCalculator : ITaxReportOperations
    {
        decimal TotalProfit();
        decimal TotalLoss();
        decimal TotalProfitLoss();
        TotalPurchasesAndSales CalculateTotalPurchasesAndSales();
    }

    public interface ITaxReportOperations
    {
        IEnumerable<string> PrintProfitSales();
        IEnumerable<string> PrintLossSales();
        //string PrintTotalProfit();
        //string PrintTotalLoss();
    }

    // ENUMS
    
    public enum FiatCurrency
    {
        Eur = 0,
        Usd,
        SwedishKrona,
        NorwegianKrone
    }


    public enum TransactionType
    {
        /// <summary>
        /// Move fiat money from bank to shares account 
        /// </summary>
        Deposit,

        /// <summary>
        /// Move fiat money from shares account to bank
        /// </summary>
        Withdrawal,
        Buy,

        /// <summary>
        /// Tax implications
        /// </summary>
        Sell,

        /// <summary>
        /// Move cryptos from wallet to proof-of-stake
        /// </summary>
        StakingWithdrawal,

        /// <summary>
        /// Cryptos arrive to proof-of-stake
        /// </summary>
        StakingDeposit,

        /// <summary>
        /// 
        /// </summary>
        StakingOperation,

        /// <summary>
        /// Receive crypto earnings from staking
        /// </summary>
        StakingDividend,

        /// <summary>
        /// Receive fiat money earnings, usually with some tax fees
        /// </summary>
        Dividend,

        Split
    }
}
