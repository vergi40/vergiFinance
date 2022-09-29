using vergiFinance.Model;

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
}
