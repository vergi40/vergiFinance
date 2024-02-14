using System.Transactions;
using vergiFinance.Model;

namespace vergiFinance.FinanceFunctions
{
    internal class AllHoldingsResult : IAllHoldingsResult
    {
        public IReadOnlyList<IHoldingsResult> AllHoldings { get; }
        public IReadOnlyDictionary<string, List<IHoldingsResult>> AllHoldingsByTicker { get; }

        public AllHoldingsResult(List<IHoldingsResult> allHoldings)
        {
            AllHoldings = allHoldings;

            var dict = new Dictionary<string, List<IHoldingsResult>>();
            foreach (var holding in allHoldings)
            {
                if (dict.ContainsKey(holding.Ticker))
                {
                    dict[holding.Ticker].Add(holding);
                }
                else
                {
                    dict.Add(holding.Ticker, new List<IHoldingsResult>() { holding });
                }
            }
            AllHoldingsByTicker = dict;
        }
    }

    internal class HoldingsResult : IHoldingsResult
    {
        public string Ticker { get; }
        public decimal AssetAmountInWallet { get; }
        public decimal AssetAmountStaked { get; }
        public decimal AssetAmountTotal { get; }

        public HoldingsResult(string ticker, decimal assetAmount)
        {
            Ticker = ticker;
            AssetAmountInWallet = assetAmount;
        }

        public HoldingsResult(string ticker, decimal assetAmountInWallet, decimal assetAmountStaked, decimal assetAmountTotal)
        {
            Ticker = ticker;
            AssetAmountInWallet = assetAmountInWallet;
            AssetAmountStaked = assetAmountStaked;
            AssetAmountTotal = assetAmountTotal;
        }
    }
}
