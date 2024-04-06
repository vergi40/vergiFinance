using Shouldly;
using vergiCommon;
using vergiFinance.Brokers;
using vergiFinance.Model;

namespace vergiFinance.UnitTests
{
    [TestFixture]
    internal class ReadTransactionsTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void ReadKrakenTransactions_Trx_TypeCountsShouldMatch()
        {
            var resFolder = Path.Combine(GetPath.ThisProject(), "Resources");
            var csv = Get.ReadCsvFile(Path.Combine(resFolder, "trx.csv"));

            var kraken = new KrakenBroker();
            var events = kraken.ReadTransactions(csv.Lines);
            var transactions = events.Transactions;

            transactions.Count(t => t.Type == TransactionType.Buy).ShouldBe(1);
            transactions.Count(t => t.Type == TransactionType.Sell).ShouldBe(1);
            transactions.Count(t => t.Type == TransactionType.StakingDividend).ShouldBe(10);
            transactions.Count(t => t.Type == TransactionType.WalletToStaking).ShouldBe(1);
            transactions.Count(t => t.Type == TransactionType.StakingToWallet).ShouldBe(1);
        }

        [Test]
        public void ReadKrakenTransactions_Aave_TypeCountsShouldMatch()
        {
            var resFolder = Path.Combine(GetPath.ThisProject(), "Resources");
            var csv = Get.ReadCsvFile(Path.Combine(resFolder, "aave.csv"));

            var kraken = new KrakenBroker();
            var events = kraken.ReadTransactions(csv.Lines);
            var transactions = events.Transactions;

            transactions.Count(t => t.Type == TransactionType.Buy).ShouldBe(3);
            transactions.Count(t => t.Type == TransactionType.Sell).ShouldBe(0);
            transactions.Count(t => t.Type == TransactionType.StakingDividend).ShouldBe(0);
            transactions.Count(t => t.Type == TransactionType.WalletToStaking).ShouldBe(0);
            transactions.Count(t => t.Type == TransactionType.StakingToWallet).ShouldBe(0);
        }
    }
}
