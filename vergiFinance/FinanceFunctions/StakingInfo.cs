using vergiFinance.Model;

namespace vergiFinance.FinanceFunctions
{
    public class StakingInfo
    {
        private List<Dividend> _dividends { get; } = new();
        private List<Withdrawal> _withdrawals { get; } = new();

        public bool HasEvents => _withdrawals.Any() || _dividends.Any(); 



        public record Withdrawal(decimal assetAmount, decimal fiatAmount, TransactionBase transaction);
        public record Dividend(decimal assetAmount, TransactionBase transaction);

        public void AddWithdrawal(TransactionBase transaction, decimal currentPriceFiat, decimal totalAssetAmount)
        {
            var item = new Withdrawal(totalAssetAmount, currentPriceFiat * totalAssetAmount, transaction);
            _withdrawals.Add(item);
        }

        public void AddDividend(TransactionBase transaction)
        {
            var item = new Dividend(transaction.AssetAmount, transaction);
            _dividends.Add(item);
        }

        public decimal TotalDividends()
        {
            return _dividends.Sum(d => d.assetAmount);
        }

        public string TotalWithdrawals()
        {
            return $"{_withdrawals.Sum(w => w.fiatAmount):F2}e, {_withdrawals.Sum(w => w.assetAmount)} units";
        }
    }
}
