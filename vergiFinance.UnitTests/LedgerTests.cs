using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using vergiCommon;
using vergiFinance.Brokers;
using vergiFinance.Brokers.Kraken.Operations;

namespace vergiFinance.UnitTests
{
    [TestFixture]
    internal class LedgerTests
    {
        private Mock<IPriceFetcher> _fetcher;

        [SetUp]
        public void Setup()
        {
            _fetcher = new Mock<IPriceFetcher>();
            _fetcher.Setup(f => f.GetCoinPriceForDate(It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.FromResult(0.077m));
        }

        [Test]
        public void TrxSalesWithStakingWithdrawals_ShouldMatch()
        {
            var resFolder = Path.Combine(GetPath.ThisProject(), "Resources");
            var csv = Get.ReadCsvFile(Path.Combine(resFolder, "trx.csv"));

            var kraken = new KrakenBroker();
            var events = kraken.ReadTransactions(csv.Lines);
            var sales = events.CalculateSales(2022, "TRX", _fetcher.Object);

            var profitLoss = sales.TotalProfitLoss();
            profitLoss.ShouldBe(356.04m, 0.001m);
            //var report = events.PrintExtendedTaxReport(2022);

            // StakedFromSpot -> StakedToEarn 11,286.681715TRX, balance 0.00000058TRX
            // UnstakedFromEarn -> UnstakedToSpot 11,395.86825TRX, balance 11,395.86825058TRX 

            // After selling TRX:
            // Sold 11,395.86825TRX for 855.5004EUR, fee 2.2243EUR
            // Balance 0.00000058TRX

            // Later received reward 1.37909TRX
            // Balance 1.37909058TRX

        }

        [Explicit("Not meant for unit testing")]
        [Test]
        public async Task FetchTickerPriceTest()
        {
            var fetcher = new PriceFetcherWithPersistence();
            var result = await fetcher.GetCoinPriceForDate("TRX", new DateTime(2022, 5, 31));

            result.ShouldBe(0.077m, 0.001m);
        }
    }
}
