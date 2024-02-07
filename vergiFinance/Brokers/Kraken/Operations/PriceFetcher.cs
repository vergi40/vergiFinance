using System.Globalization;
using System.Transactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using vergiFinance.Model;
using vergiFinance.Persistence;

namespace vergiFinance.Brokers.Kraken.Operations
{
    public interface IPriceFetcher
    {
        /// <summary>
        /// HTTP Get single day price for given ticker.
        /// https://www.coingecko.com/en/api
        /// </summary>
        Task<decimal> GetCoinPriceForDate(string ticker, DateTime date);
    }

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
            {"ALGO.S", "algorand"},
            {"TRX", "tron"},
            {"TRX.S", "tron"},
        };

        private HttpClient _client { get; } = new HttpClient();
        private string _urlBase { get; } = "https://api.coingecko.com/api/v3/";
        private readonly Persistence.Persistence _stakingPersistence;

        private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("fi-FI");

        public PriceFetcher()
        {
            _stakingPersistence = new Persistence.Persistence(new DatabaseLite());
        }

        /// <summary>
        /// NOTE: Not good design to update one property in mutable items. But for PoC it'll do
        /// </summary>
        /// <param name="stakingRewards"></param>
        /// <returns></returns>
        public async Task FillDayUnitPrice(List<TransactionBase> stakingRewards)
        {
            foreach (var transaction in stakingRewards)
            {
                Console.WriteLine($"Fetching price for {transaction.Ticker} at {transaction.TradeDate}...");

                if (_stakingPersistence.TryLoadSingleStakingData(transaction.Ticker, transaction.TradeDate,
                        out var dto))
                {
                    transaction.DayUnitPrice = dto.DayUnitPrice;

                    Console.WriteLine($"Success [from db]: {dto.DayUnitPrice:G6}e");
                    continue;
                }

                try
                {
                    var priceAtDate = await GetCoinPriceForDate(transaction.Ticker, transaction.TradeDate);
                    transaction.DayUnitPrice = priceAtDate;
                }
                catch (HttpRequestException e)
                {
                    // TODO log
                    // Commonly throws when too many requests
                    Console.WriteLine(e.ToString());
                    throw;
                }

                Console.WriteLine($"Success [from http api]: {transaction.DayUnitPrice:G6}e");
                SaveUnitPrice(transaction);
                Console.WriteLine("--> Saved to db");
            }
        }

        public async Task<decimal> GetCoinPriceForDate(string ticker, DateTime date)
        {
            var id = TickerToId[ticker];

            // https://api.coingecko.com/api/v3/coins/ethereum/history
            var req = _urlBase + $"coins/{id}/history?date={date.Date:dd-MM-yyyy}";

            var response = await _client.GetAsync(req);
            response.EnsureSuccessStatusCode();
            return DeserializeCoinMarketData(await response.Content.ReadAsStringAsync());
        }

        public decimal DeserializeCoinMarketData(string jsonString)
        {
            // https://www.newtonsoft.com/json/help/html/SerializingJSONFragments.htm
            var jObject = JObject.Parse(jsonString);

            var marketData = jObject["market_data"];
            var eurString = marketData["current_price"]["eur"].ToString();
            // Gives decimal as 120,11 or 120.11
            if(eurString.Contains('.')) return Convert.ToDecimal(eurString, CultureInfo.InvariantCulture);
            return Convert.ToDecimal(eurString, _culture);
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

        public void SaveUnitPrice(TransactionBase stakingRewardTransaction)
        {
            var dto = new StakingDto()
            {
                DayUnitPrice = stakingRewardTransaction.DayUnitPrice,
                BrokerId = stakingRewardTransaction.Id,
                Ticker = stakingRewardTransaction.Ticker,
                TradeDate = stakingRewardTransaction.TradeDate
            };

            _stakingPersistence.SaveSingleStakingData(dto);
        }

        public void SaveUnitPrices(IEnumerable<TransactionBase> stakingRewards)
        {
            foreach (var transaction in stakingRewards)
            {
                SaveUnitPrice(transaction);
            }
        }
    }

    public record CoinId(string id, string symbol, string name);

}
