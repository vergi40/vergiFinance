using vergiFinance.Brokers.Kraken.Operations;
using vergiFinance.Model;

namespace vergiFinance.FinanceFunctions
{
    internal class SalesResult : ISalesResult
    {
        private List<SalesUnitInformation> _allSales { get; }
        private int _year { get; }
        private SalesPrinter _printer { get; }

        public StakingInfo Staking { get; } = new();


        public SalesResult(List<SalesUnitInformation> allSales, int year)
        {
            _allSales = allSales;
            _year = year;
            _printer = SalesPrinterFactory.CreateEng();
        }

        public SalesResult(List<SalesUnitInformation> allSales, int year, StakingInfo stakingInfo)
        {
            _allSales = allSales;
            _year = year;
            _printer = SalesPrinterFactory.CreateEng();
            Staking = stakingInfo;
        }

        /// <summary>
        /// Tax implications
        /// </summary>
        public bool ContainsSellEvents { get; private set; }
        /// <summary>
        /// Tax implications
        /// </summary>
        public bool ContainsStakingWithdrawals { get; private set; }

        public IEnumerable<string> PrintProfitSales()
        {
            var profitSales = _allSales.Where(s => s.Type is SalesType.Profit or SalesType.StakingWithdrawal && s.TradeDate.Year == _year);
            return _printer.PrintProfitSales(profitSales);
        }

        public IEnumerable<string> PrintLossSales()
        {
            var lossSales = _allSales.Where(s => s.Type == SalesType.Loss && s.TradeDate.Year == _year);
            return _printer.PrintLossSales(lossSales);
        }

        public decimal TotalProfit()
        {
            return _allSales.Where(s => s.Type is SalesType.Profit or SalesType.StakingWithdrawal && s.TradeDate.Year == _year).Sum(s => s.ProfitLoss);
        }

        public decimal TotalLoss()
        {
            return _allSales.Where(s => s.Type == SalesType.Loss && s.TradeDate.Year == _year).Sum(s => s.ProfitLoss);
        }

        public decimal TotalProfitLoss()
        {
            return _allSales.Where(s => s.TradeDate.Year == _year).Sum(s => s.ProfitLoss);
        }

        public TotalPurchasesAndSales CalculateTotalPurchasesAndSales()
        {
            var lossList = _allSales.Where(s => s.Type == SalesType.Loss).ToList();
            var lossPurchases = lossList.Sum(s => s.BoughtTotalPrice);
            var lossSales = lossList.Sum(s => s.SoldTotalPrice);

            var profitList = _allSales.Where(s => s.Type is SalesType.Profit or SalesType.StakingWithdrawal).ToList();
            var profitPurchases = profitList.Sum(s => s.BoughtTotalPrice);
            var profitSales = profitList.Sum(s => s.SoldTotalPrice);

            return new TotalPurchasesAndSales(lossPurchases, lossSales, profitPurchases, profitSales);
        }
    }
}
