using System.Transactions;
using vergiFinance.Model;

namespace vergiFinance.FinanceFunctions
{
    internal class AllHoldingsResult : IAllHoldingsResult
    {
        public IReadOnlyList<IHoldingsResult> AllHoldings { get; }
        public IReadOnlyDictionary<string, IHoldingsResult> AllHoldingsByTicker { get; }

        public AllHoldingsResult(List<IHoldingsResult> allHoldings)
        {
            AllHoldings = allHoldings;

            var dict = new Dictionary<string, IHoldingsResult>();
            foreach (var holding in allHoldings)
            {
                dict[holding.Ticker] = holding;
            }
            AllHoldingsByTicker = dict;
        }
    }

    internal class HoldingsResult : IHoldingsResult
    {
        public string Ticker { get; }
        public decimal AssetAmountInWallet { get; }
        public decimal AssetAmountStaked { get; }
        public decimal AssetAmountTotal => AssetAmountInWallet + AssetAmountStaked;

        public HoldingsResult(string ticker, decimal assetAmount)
        {
            Ticker = ticker;
            AssetAmountInWallet = assetAmount;
        }

        public HoldingsResult(string ticker, decimal assetAmountInWallet, decimal assetAmountStaked)
        {
            Ticker = ticker;
            AssetAmountInWallet = assetAmountInWallet;
            AssetAmountStaked = assetAmountStaked;
        }
    }
}
