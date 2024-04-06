using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiFinance.FinanceFunctions
{
    internal class Account
    {
        public int EventCount { get; private set; }
        public decimal Amount { get; private set; }

        public void Add(decimal amount)
        {
            Amount += amount;
            EventCount++;
        }

        public void Subtract(decimal amount)
        {
            Amount -= amount;
            EventCount++;
        }

        public override string ToString()
        {
            return $"Amount: {Amount:F2}, event count: {EventCount}";
        }
    }

    internal class KrakenAccounts
    {
        public Account Fiat { get; } = new();
        public Account Crypto { get; } = new();

        /// <summary>
        /// Amount moved from crypto to stake wallet. No dividends
        /// </summary>
        public Account Staking { get; } = new();

        /// <summary>
        /// Asset type = crypto
        /// </summary>
        public Account StakingDividends { get; } = new();
    }
}
