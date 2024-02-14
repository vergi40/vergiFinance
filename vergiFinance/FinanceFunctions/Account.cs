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
}
