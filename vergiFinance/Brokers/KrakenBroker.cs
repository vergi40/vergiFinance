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
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var action = RawTransaction.Parse(line); 
                transactions.Add(action);
            }

            var krakenLog = new KrakenLog(transactions);
            krakenLog.Transactions.Sort((a, b) => a.TradeDate.CompareTo(b.TradeDate));

            return krakenLog;
        }
    }
}
