using System.Globalization;
using Shouldly;
using vergiFinance.Brokers.Kraken.Operations;

namespace vergiFinance.UnitTests
{
    public class Tests
    {
        private CoinGeckoPriceFetcher _coinGecko;

        [SetUp]
        public void Setup()
        {
            _coinGecko = new CoinGeckoPriceFetcher();
        }

        [Test]
        public void DeserializeCoinIdJson()
        {
            var jsonFilePath = GetTestJson("coinlist.json");
            var jsonString = File.ReadAllText(jsonFilePath);

            var coins = CoinGeckoPriceFetcher.DeserializeCoinList(jsonString);

            Assert.That(coins, Has.Exactly(13487).Items);
        }

        [Test]
        public void DeserializeCoinMarketDataJson()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var jsonFilePath = GetTestJson("coinhistory.json");
            var jsonString = File.ReadAllText(jsonFilePath);

            var amount = _coinGecko.DeserializeCoinMarketData(jsonString);

            //Assert.AreEqual(120.11, (double)amount, 0.1);
            amount.ShouldBe(120.11m, 0.1m);
        }

        private void AssertDecimalWithDelta(decimal expected, decimal actual, decimal delta = 0.1m)
        {
            Assert.That(expected, Is.InRange(actual - delta, actual + delta));
        }

        private static string GetTestJson(string jsonFileName)
        {
            var resFolder = TestUtils.GetResourcesPath();
            return Path.Combine(resFolder, "CoinGecko", jsonFileName);
        }
    }
}