using vergiCommon;

namespace vergiFinance.UnitTests
{
    public class Tests
    {

        [Test]
        public void SerializeJson()
        {
            var jsonFilePath = Path.Combine(Constants.MyDocumentsTempLocation, "coinlist.json");
            var jsonString = File.ReadAllText(jsonFilePath);

            PriceFetcher.DeserializeCoinList(jsonString);
        }
    }
}