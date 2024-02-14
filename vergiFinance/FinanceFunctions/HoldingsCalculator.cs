using vergiFinance.Model;

namespace vergiFinance.FinanceFunctions
{
    internal class HoldingsCalculator
    {
        public IHoldingsResult CalculateHoldings(string ticker, List<TransactionBase> transactions)
        {
            var cryptoWallet = new Account();

            foreach (var transaction in GetBuySellTransactions(transactions))
            {
                if (transaction.Type == TransactionType.Buy)
                {
                    cryptoWallet.Add(transaction.AssetAmount);
                }
                else if (transaction.Type == TransactionType.Sell)
                {
                    cryptoWallet.Subtract(transaction.AssetAmount);
                }
            }

            return new HoldingsResult(ticker, cryptoWallet.Amount);
        }

        public IHoldingsResult CalculateHoldingsWithStaking(string ticker, List<TransactionBase> transactions)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Buy, sell, to staking, from staking
        /// </summary>
        private IEnumerable<TransactionBase> GetTransactionsForTaxCalculations(
            IEnumerable<TransactionBase> transactions)
        {
            return transactions
                .Where(t => t.Type is
                    TransactionType.Buy or TransactionType.Sell or
                    TransactionType.WalletToStaking or TransactionType.StakingToWallet or
                    TransactionType.StakingDividend);
        }

        private IEnumerable<TransactionBase> GetBuySellTransactions(
            IEnumerable<TransactionBase> transactions)
        {
            return transactions
                .Where(t => t.Type is
                    TransactionType.Buy or TransactionType.Sell);
        }
    }

    internal class HoldingsUnitInformation
    {

    }
}
