namespace vergiFinance.FinanceFunctions
{
    internal static class SalesPrinterFactory
    {
        public static SalesPrinter CreateEng()
        {
            return new SalesPrinter("e");
        }
    }

    internal class SalesPrinter
    {
        private string _currencySymbol { get; }

        public SalesPrinter(string currencySymbol)
        {
            _currencySymbol = currencySymbol;
        }

        public IEnumerable<string> PrintProfitSales(IEnumerable<SalesUnitInformation> profitSales)
        {
            foreach (var singleSale in profitSales)
            {
                if (singleSale.Type == SalesType.StakingWithdrawal)
                {
                    var assetAmount = $"{singleSale.AssetAmount:G8}";
                    var message =
                        $"{singleSale.TradeDate:dd/MM/yyyy} " +
                        $"staking withdrawal (staking->spot)   " +
                        $"amount: {assetAmount}   " +
                        $"unit price on current date: {UnitPricePrettify(singleSale.SoldUnitPrice)}   " +
                        $"fiat value on current date: {PricePrettify(singleSale.SoldTotalPrice),-10}";
                    yield return message;
                }
                else
                {
                    var message =
                        $"{singleSale.TradeDate:dd/MM/yyyy} " +
                        $"sell price {PricePrettify(singleSale.SoldTotalPrice),-10}" +
                        $"profit {PricePrettify(singleSale.ProfitLoss),-10}" +
                        $"unit price {UnitPricePrettify(singleSale.SoldUnitPrice)}{_currencySymbol}    " +
                        $"original unit price {UnitPricePrettify(singleSale.OriginalUnitPrice)}{_currencySymbol}";
                    yield return message;
                }
            }
        }

        public IEnumerable<string> PrintLossSales(IEnumerable<SalesUnitInformation> lossSales)
        {
            foreach (var singleSale in lossSales)
            {
                var message =
                    $"{singleSale.TradeDate:dd/MM/yyyy} " +
                    $"sell price {PricePrettify(singleSale.SoldTotalPrice),-10}" +
                    $"loss   {PricePrettify(singleSale.ProfitLoss),-10}" +
                    $"unit price {UnitPricePrettify(singleSale.SoldUnitPrice)}{_currencySymbol}    " +
                    $"original unit price {UnitPricePrettify(singleSale.OriginalUnitPrice)}{_currencySymbol}";
                yield return message;
            }
        }

        private string PricePrettify(decimal price)
        {
            return $"{price:F2}e";
        }

        private string UnitPricePrettify(decimal unitPrice)
        {
            var unitPriceString = $"{unitPrice:F2}";
            if (unitPrice < 1) unitPriceString = $"{unitPrice:F4}";

            return unitPriceString;
        }

        public string PrintTotalProfit(IEnumerable<SalesUnitInformation> profitSales)
        {
            var total = profitSales.Sum(s => s.ProfitLoss);
            return $"{total:F2}{_currencySymbol}";
        }
    }
}
