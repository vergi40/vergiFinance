using vergiFinance.Brokers.Kraken.Operations;
using vergiFinance.FinanceFunctions;

namespace vergiFinance.Model
{
    // OPERATORS
    
    public interface ISalesCalculator
    {
        StakingInfo Staking { get; }
    }

    public interface ISalesResult : ITaxReportOperations
    {
        StakingInfo Staking { get; }
        decimal TotalProfit();
        decimal TotalLoss();
        decimal TotalProfitLoss();
        TotalPurchasesAndSales CalculateTotalPurchasesAndSales();
    }

    public interface IAllHoldingsResult
    {
        IReadOnlyList<IHoldingsResult> AllHoldings { get; }
        IReadOnlyDictionary<string, List<IHoldingsResult>> AllHoldingsByTicker { get; }
    }

    public interface IHoldingsResult
    {
        /// <summary>
        /// NOK, NVDA, ETH, 
        /// </summary>
        public string Ticker { get; }

        /// <summary>
        /// Amount of units under control. Non-negative.
        /// Kpl määrä. Esim 8 kpl osakkeita. 0.001 kpl kryptoa
        /// </summary>
        public decimal AssetAmountInWallet { get; }

        /// <summary>
        /// Amount of units, locked in staking. Non-negative.
        /// Kpl määrä. Esim 8 kpl osakkeita. 0.001 kpl kryptoa
        /// </summary>
        public decimal AssetAmountStaked { get; }

        /// <summary>
        /// Total amount of units. Non-negative.
        /// Kpl määrä. Esim 8 kpl osakkeita. 0.001 kpl kryptoa
        /// </summary>
        public decimal AssetAmountTotal { get; }
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
        /// Uninitialized, error
        /// </summary>
        Noop,

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
        /// Two step. Crypto taken from crypto wallet. Crypto.S arrives to staking wallet.
        /// Asset name supplied without ".S"
        /// </summary>
        WalletToStaking,

        /// <summary>
        /// Tax implications. Two step. Crypto.S taken from staking wallet. Crypto arrives to crypto wallet.
        /// Asset name supplied without ".S"
        /// </summary>
        StakingToWallet,

        /// <summary>
        /// Move cryptos from crypto wallet to proof-of-stake
        /// </summary>
        StakingWithdrawal,

        /// <summary>
        /// Cryptos arrive to proof-of-stake
        /// </summary>
        StakingDeposit,

        /// <summary>
        /// Placeholder - duplicate to StakingDividend
        /// </summary>
        StakingOperation,

        /// <summary>
        /// Receive crypto earnings from staking to staking wallet
        /// </summary>
        StakingDividend,

        /// <summary>
        /// Receive fiat money earnings, usually with some tax fees
        /// </summary>
        Dividend,

        Split
    }
}
