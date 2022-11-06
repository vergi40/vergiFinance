using vergiFinance.Brokers.Kraken.Operations;
using vergiFinance.FinanceFunctions;

namespace vergiFinance.Model
{
    // OPERATORS
    
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
