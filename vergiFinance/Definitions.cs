using System.Collections.Generic;

namespace vergiFinance
{
    /// <summary>
    /// Broker that was used for transactions, e.g. Nordnet, Kraken ...
    /// </summary>
    public interface IBrokerService
    {
        public IEventLog ReadTransactions(IReadOnlyList<string> lines);
    }

    /// <summary>
    /// Contains all strong-typed transactions and manipulation methods
    /// </summary>
    public interface IEventLog
    {
        List<TransactionBase> Transactions { get; set; }


        string PrintExtendedTaxReport(int year);
        List<int> TransactionYearSpan();
    }
    
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
