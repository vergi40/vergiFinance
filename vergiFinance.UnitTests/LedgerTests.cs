using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiCommon;
using vergiFinance.Brokers;

namespace vergiFinance.UnitTests
{
    [TestFixture]
    internal class LedgerTests
    {
        [Test]
        public void StakingToSpot()
        {
            var resFolder = Path.Combine(GetPath.ThisProject(), "Resources");
            var csv = Get.ReadCsvFile(Path.Combine(resFolder, "trx.csv"));

            var kraken = new KrakenBroker();
            var events = kraken.ReadTransactions(csv.Lines);
            var report = events.PrintExtendedTaxReport(2022);

            // StakedFromSpot -> StakedToEarn 11,286.681715TRX, balance 0.00000058TRX
            // UnstakedFromEarn -> UnstakedToSpot 11,395.86825TRX, balance 11,395.86825058TRX 

            // After selling TRX:
            // Sold 11,395.86825TRX for 855.5004EUR, fee 2.2243EUR
            // Balance 0.00000058TRX

            // Later received reward 1.37909TRX
            // Balance 1.37909058TRX

        }
    }
}
