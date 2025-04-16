using vergiFinance.Brokers.Kraken;

namespace vergiFinance.Brokers
{
    public class KrakenBroker : IBrokerService
    {
        public IEventLog ReadTransactions(IReadOnlyList<string> lines)
        {
            var transactions = new List<RawTransaction>();
            foreach (var line in lines)
            {
                if (line.StartsWith("\"txid\"", StringComparison.InvariantCulture))
                {
                    // CSV definition line
                    continue;
                }
                if (string.IsNullOrWhiteSpace(line)) continue;

                var action = CsvToRawTransactionParser.Parse(line); 
                transactions.Add(action);
            }
            return EventLogFactory.CreateKrakenLog(transactions);
        }
    }
}
