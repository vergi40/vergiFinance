using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
using vergiFinance.Brokers.Kraken;
using vergiFinance.Persistence;

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

                var action = RawTransaction.Parse(line); 
                transactions.Add(action);
            }
            return EventLogFactory.CreateKrakenLog(transactions);
        }
    }
}
