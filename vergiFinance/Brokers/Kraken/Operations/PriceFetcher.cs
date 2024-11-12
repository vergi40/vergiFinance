using System.Globalization;
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
        /// </summary>
        /// <returns>Valid price for successful query. null for exception</returns>
        Task<decimal?> GetCoinPriceForDate(string ticker, DateTime date);
    }

    public class PriceFetcher : IPriceFetcher
    {
        private readonly IPriceFetcherHttpImplementation _fetcherImplementation;

        public PriceFetcher(IPriceFetcherHttpImplementation fetcherImplementation)
        {
            _fetcherImplementation = fetcherImplementation;
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

                try
                {
                    var priceAtDate = await _fetcherImplementation.GetCoinPriceForDate(transaction.Ticker, transaction.TradeDate);
                    transaction.DayUnitPrice = priceAtDate ?? -9999m;
                }
                catch (HttpRequestException e)
                {
                    // TODO log
                    // Commonly throws when too many requests
                    Console.WriteLine(e.ToString());
                    throw;
                }

                Console.WriteLine($"Success [from http api]: {transaction.DayUnitPrice:G6}e");
            }
        }

        public async Task<decimal?> GetCoinPriceForDate(string ticker, DateTime date)
        {
            try
            {
                var result = await _fetcherImplementation.GetCoinPriceForDate(ticker, date);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }

    public class PriceFetcherWithPersistence : IPriceFetcher
    {
        private readonly IPriceFetcherHttpImplementation _fetcherImplementation;
        private readonly Persistence.Persistence _stakingPersistence;

        public PriceFetcherWithPersistence(IPriceFetcherHttpImplementation fetcherImplementation, 
            Persistence.Persistence stakingPersistence)
        {
            _fetcherImplementation = fetcherImplementation;
            _stakingPersistence = stakingPersistence;
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
                    var priceAtDate = await _fetcherImplementation.GetCoinPriceForDate(transaction.Ticker, transaction.TradeDate);
                    transaction.DayUnitPrice = priceAtDate ?? -9999m;
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

        public async Task<decimal?> GetCoinPriceForDate(string ticker, DateTime date)
        {
            // See if already persisted
            if (_stakingPersistence.TryLoadSingleStakingData(ticker, date.Date,
                    out var existingDto))
            {
                var dayUnitPrice = existingDto.DayUnitPrice;

                Console.WriteLine($"Success [from db]: {existingDto.DayUnitPrice:G6}e");
                return dayUnitPrice;
            }

            var unitPrice = await _fetcherImplementation.GetCoinPriceForDate(ticker, date);
            await Task.Delay(500);

            var dto = new StakingDto()
            {
                DayUnitPrice = unitPrice ?? -9999m,
                BrokerId = "",
                Ticker = ticker,
                TradeDate = date.Date
            };

            _stakingPersistence.SaveSingleStakingData(dto);
            return unitPrice;
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

    public static class CoinGeckoConstants
    {
        /// <summary>
        /// CoinGecko uses own id's
        /// </summary>
        public static Dictionary<string, string> TickerToId { get; } = new()
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
            {"ADA", "ada"},
            {"ADA.S", "ada"},
            {"XTZ", "tezos"},
            {"XTZ.S", "tezos"},
        };
    }
}
