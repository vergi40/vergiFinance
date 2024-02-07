using vergiFinance.Brokers.Kraken;

namespace vergiFinance.Model
{
    public static class TransactionFactory
    {
        public static TransactionBase CreateWithdrawal()
        {
            throw new NotImplementedException();
        }

        public static TransactionBase CreateDeposit()
        {
            throw new NotImplementedException();
        }

        public static TransactionBase Create(TransactionType type, FiatCurrency currency, string ticker, decimal assetAmount, decimal pricePerUnit, DateTime time)
        {
            var transaction = new TransactionBase()
            {
                Type = type,
                FiatCurrency = currency,
                Ticker = ticker,
                AssetAmount = assetAmount,
                AssetUnitPrice = pricePerUnit,
                TradeDate = time
            };
            return transaction;
        }

        public static TransactionBase CreateBuy(FiatCurrency currency, string ticker, decimal assetAmount, decimal pricePerUnit, DateTime time)
        {
            var transaction = new TransactionBase()
            {
                Type = TransactionType.Buy,
                FiatCurrency = currency,
                Ticker = ticker,
                AssetAmount = assetAmount,
                AssetUnitPrice = pricePerUnit,
                TradeDate = time
            };
            return transaction;
        }
        
        private static TransactionBase CreateSell(FiatCurrency currency, string ticker, decimal assetAmount, decimal pricePerUnit, DateTime time)
        {
            var transaction = new TransactionBase()
            {
                Type = TransactionType.Sell,
                FiatCurrency = currency,
                Ticker = ticker,
                AssetAmount = assetAmount,
                AssetUnitPrice = pricePerUnit,
                TradeDate = time
            };
            return transaction;
        }

        /// <summary>
        /// Form generic transaction data from kraken transactions
        /// </summary>
        /// <param name="trade1"></param>
        /// <param name="trade2"></param>
        /// <returns></returns>
        public static TransactionBase CreateKrakenTrade(RawTransaction trade1, RawTransaction trade2)
        {
            (RawTransaction fiat, RawTransaction crypto) = (trade1, trade2);
            if (trade2.Asset.Contains("EUR")) (fiat, crypto) = (trade2, trade1);

            var type = TransactionType.Buy;
            if (fiat.Amount > 0m) type = TransactionType.Sell;

            var fiatAmount = Math.Abs(fiat.Amount);
            var assetAmount = Math.Abs(crypto.Amount);
            var pricePerUnit = Math.Abs(fiatAmount / assetAmount);

            var transaction = Create(type, FiatCurrency.Eur, crypto.Asset, assetAmount, pricePerUnit, fiat.Time);
            transaction.Market = "KRAKEN";

            if (Math.Abs(fiat.Fee) > 0m)
            {
                transaction.Fee = Math.Abs(fiat.Fee);
            }
            else if (Math.Abs(crypto.Fee) > 0m)
            {
                // Fee logging depends if they are paid from crypto of from fiat
                // Crypto fees are logged a bit weird. It should be added back to asset amount and that should be used to calculate fee in fiat
                transaction.AssetAmount += Math.Abs(crypto.Fee);
                transaction.AssetUnitPrice = Math.Abs(fiatAmount / transaction.AssetAmount);
                transaction.Fee = transaction.AssetUnitPrice * Math.Abs(crypto.Fee);
            }


            return transaction;
        }

        /// <summary>
        /// Create single transfer item from 4 lines of data
        /// </summary>
        /// <param name="transferType"></param>
        /// <param name="isWithdrawalSide"></param>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static TransactionBase CreateStakingTransfer(TransactionType transferType, bool isWithdrawalSide, RawTransaction item1, RawTransaction item2)
        {
            // "","BU2WOCW-TAKWRI-NPGRSY","2022-04-02 19:38:18","withdrawal","","currency","TRX",-11286.68171500,0.00000000,""
            // "id1","BU2WOCW-TAKWRI-NPGRSY","2022-04-02 19:39:55","transfer","spottostaking","currency","TRX",-11286.68171500,0.00000000,0.00000058
            // "","RU2HAZV-UZKGZM-K2VGQI","2022-04-02 19:41:06","deposit","","currency","TRX.S",11286.68171500,0.00000000,""
            // "id1","RU2HAZV-UZKGZM-K2VGQI","2022-04-02 19:41:44","transfer","stakingfromspot","currency","TRX.S",11286.68171500,0.00000000,11286.68171500

            // "","BU3HP7C-4AQSGP-NPNBBB","2022-05-31 19:39:07","withdrawal","","currency","TRX.S",-11395.86825000,0.00000000,""
            // "id1","BU3HP7C-4AQSGP-NPNBBB","2022-05-31 19:40:51","transfer","stakingtospot","currency","TRX.S",-11395.86825000,0.00000000,0.00000000
            // "","RUZPX6Q-QE5IZI-DDHARS","2022-05-31 19:41:47","deposit","","currency","TRX",11395.86825000,0.00000000,""
            // "id1","RUZPX6Q-QE5IZI-DDHARS","2022-05-31 19:42:30","transfer","spotfromstaking","currency","TRX",11395.86825000,0.00000000,11395.86825058

            if (!isWithdrawalSide) throw new InvalidOperationException("Invalid data to create staking transfer");
            if (transferType == TransactionType.WalletToStaking)
            {
                // Ensure order
                var (withdrawal, transfer) = (item1, item2);
                if (item1.TypeAsString == "transfer") (transfer, withdrawal) = (withdrawal, transfer);

                if (transfer.Asset.EndsWith(".S"))
                {
                    throw new InvalidOperationException("Logical error");
                }
                return TransactionFactory.Create(transferType, FiatCurrency.Eur, transfer.Asset,
                    Math.Abs(transfer.Amount), 0m, withdrawal.Time);
            }
            else if (transferType == TransactionType.StakingToWallet)
            {
                // Ensure order
                var (withdrawal, transfer) = (item1, item2);
                if (item1.TypeAsString == "transfer") (transfer, withdrawal) = (withdrawal, transfer);

                var ticker = transfer.Asset;
                if (ticker.EndsWith(".S"))
                {
                    ticker = ticker.Substring(0, ticker.Length - 2);
                }
                return TransactionFactory.Create(transferType, FiatCurrency.Eur, ticker,
                    Math.Abs(transfer.Amount), 0m, withdrawal.Time);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}