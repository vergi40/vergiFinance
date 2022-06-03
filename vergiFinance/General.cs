using vergiFinance.Brokers;

namespace vergiFinance
{
    // Implement generic methods to handle any kind of transaction

    public static class General
    {
        public static IEventLog ReadTransactions(IReadOnlyList<string> lines)
        {
            // TODO IoC binding
            IBrokerService broker = new Kraken();

            return broker.ReadTransactions(lines);
        }
    }
}
