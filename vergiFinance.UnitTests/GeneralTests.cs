using vergiCommon;

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

        [Test]
        public void DeserializeCoinIdJson()
        {
            var jsonFilePath = Path.Combine(Constants.MyDocumentsTempLocation, "coinlist.json");
            var jsonString = File.ReadAllText(jsonFilePath);

            PriceFetcher.DeserializeCoinList(jsonString);
        }

        [Test]
        public void DeserializeCoinMarketDatason()
        {
            var jsonFilePath = Path.Combine(Constants.MyDocumentsTempLocation, "coinhistory.json");
            var jsonString = File.ReadAllText(jsonFilePath);

            var amount = _fetcher.DeserializeCoinMarketData(jsonString);

            Assert.AreEqual(120.11, (double)amount, 0.1);
        }
    }
}