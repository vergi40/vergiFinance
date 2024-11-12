using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace vergiFinance.Brokers.Kraken.Operations
{
    /// <summary>
    /// Abstract the implementation that communicates with some API, as APIs seem
    /// to quickly turn to paid service
    /// </summary>
    public interface IPriceFetcherHttpImplementation
    {
        /// <summary>
        /// HTTP Get single day price for given ticker.
        /// </summary>
        /// <returns>Valid price for successful query. null for exception</returns>
        Task<decimal?> GetCoinPriceForDate(string ticker, DateTime date);
    }

    /// <summary>
    /// CoinGecko currently let's retrieve max 365d old market data
    /// </summary>
    public class CoinGeckoPriceFetcher : IPriceFetcherHttpImplementation
    {
        private string _urlBase { get; } = "https://api.coingecko.com/api/v3/";

        private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("fi-FI");
        private HttpClient _client { get; } = new HttpClient();

        public async Task<decimal?> GetCoinPriceForDate(string ticker, DateTime date)
        {
            var id = CoinGeckoConstants.TickerToId[ticker];

            // https://api.coingecko.com/api/v3/coins/ethereum/history
            var req = _urlBase + $"coins/{id}/history?date={date.Date:dd-MM-yyyy}";

            var response = await _client.GetAsync(req);
            response.EnsureSuccessStatusCode();
            await Task.Delay(500);
            return DeserializeCoinMarketData(await response.Content.ReadAsStringAsync());
        }

        private decimal DeserializeCoinMarketData(string jsonString)
        {
            // https://www.newtonsoft.com/json/help/html/SerializingJSONFragments.htm
            var jObject = JObject.Parse(jsonString);

            var marketData = jObject["market_data"];
            var eurString = marketData["current_price"]["eur"].ToString();
            // Gives decimal as 120,11 or 120.11
            if (eurString.Contains('.')) return Convert.ToDecimal(eurString, CultureInfo.InvariantCulture);
            return Convert.ToDecimal(eurString, _culture);
        }

        [Obsolete("Seems something used for testing in console once.")]
        public static List<CoinId> DeserializeCoinList(string jsonString)
        {
            var arrays = JArray.Parse(jsonString);

            var result = new List<CoinId>();
            foreach (var array in arrays)
            {
                var coin = JsonConvert.DeserializeObject<CoinId>(array.ToString());
                result.Add(coin);
            }

            var eth = result.FirstOrDefault(r => r.id.Equals("ethereum", StringComparison.InvariantCultureIgnoreCase));
            var algo = result.FirstOrDefault(r => r.id.Equals("algorand", StringComparison.InvariantCultureIgnoreCase));

            var eths = result.Where(r => r.name.Contains("ethereum", StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            return result;
        }
    }
}
