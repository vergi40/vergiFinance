using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace vergiFinance
{
    public class PriceFetcher
    {
        /// <summary>
        /// CoinGecko uses own id's
        /// </summary>
        private Dictionary<string, string> TickerToId { get; } = new()
        {
            {"XETH", "ethereum"},
            {"ETH", "ethereum"},
            {"ETH.S", "ethereum"},
            {"ETH2", "ethereum"},
            {"ETH2.S", "ethereum"},
            {"ALGO", "algorand"},
            {"ALGO.S", "algorand"}
        };

        private HttpClient _client { get; } = new HttpClient();

        public async Task FillDayUnitPrice(List<TransactionBase> stakingRewards)
        {
            var urlBase = "https://api.coingecko.com/api/v3/";
            var coinReq = "coins/";

            var coinList = "https://api.coingecko.com/api/v3/coins/list";

            var response = await _client.GetAsync(coinList);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            

        }

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

    public record CoinId (string id, string symbol, string name);

}
