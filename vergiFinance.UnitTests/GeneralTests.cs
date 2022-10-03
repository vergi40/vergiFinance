using System.Globalization;
using vergiCommon;
using vergiFinance.Brokers.Kraken.Operations;
using vergiFinance.Model;

namespace vergiFinance.UnitTests
{
    public class Tests
    {
        private PriceFetcher _fetcher { get; set; }

        [SetUp]
        public void Setup()
        {
            _fetcher = new PriceFetcher();
        }

        /// <summary>
        /// TODO use generic or test resource 
        /// </summary>
        [Test]
        public void DeserializeCoinIdJson()
        {
            var jsonFilePath = Path.Combine(Constants.MyDocumentsTempLocation, "coinlist.json");
            var jsonString = File.ReadAllText(jsonFilePath);

            var coins = PriceFetcher.DeserializeCoinList(jsonString);

            Assert.That(coins, Has.Exactly(13487).Items);
        }

        /// <summary>
        /// TODO use generic or test resource 
        /// </summary>
        [Test]
        public void DeserializeCoinMarketDataJson()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var jsonFilePath = Path.Combine(Constants.MyDocumentsTempLocation, "coinhistory.json");
            var jsonString = File.ReadAllText(jsonFilePath);

            var amount = _fetcher.DeserializeCoinMarketData(jsonString);

            //Assert.AreEqual(120.11, (double)amount, 0.1);
            AssertDecimalWithDelta(amount, 120.11m);
        }

        private void AssertDecimalWithDelta(decimal expected, decimal actual, decimal delta = 0.1m)
        {
            Assert.That(expected, Is.InRange(actual - delta, actual + delta));
        }
    }
}