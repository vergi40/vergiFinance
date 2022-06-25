using System;
using vergiFinance.Brokers.Kraken;

namespace vergiFinance
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

        private static TransactionBase CreateBuy(FiatCurrency currency, string ticker, decimal assetAmount, decimal pricePerUnit, DateTime time)
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

        public static TransactionBase CreateStakingTransfer(TransactionType transferType, RawTransaction item1, RawTransaction item2)
        {
            // "",						"BUOCI3H-OT5F7V-CMBJ7A","2021-07-20 10:22:11","withdrawal","","currency","XETH",-0.3000000000,0.0000000000,""
            // "",						"RUNDM24-Q3U73U-UPTDYL","2021-07-20 10:25:39","deposit","","currency","ETH2.S",0.3000000000,0.0000000000,""
            // "L6I7Z5-75SMK-MF6FOA",	"BUOCI3H-OT5F7V-CMBJ7A","2021-07-20 10:25:40","transfer","spottostaking","currency","XETH",-0.3000000000,0.0000000000,0.0274338800
            // "LSIGU5-BHE2C-BLCKQV",	"RUNDM24-Q3U73U-UPTDYL","2021-07-21 00:49:21","transfer","stakingfromspot","currency","ETH2.S",0.3000000000,0.0000000000,0.9000000000
            if (transferType == TransactionType.StakingDeposit)
            {
                var (deposit, transfer) = (item1, item2);
                if (item1.TypeAsString == "transfer")
                {
                    (transfer, deposit) = (deposit, transfer);
                }

                return TransactionFactory.Create(transferType, FiatCurrency.Eur, transfer.TypeAsString,
                    Math.Abs(transfer.Amount), 0m, transfer.Time);
            }
            else if (transferType == TransactionType.StakingWithdrawal)
            {
                var (withdrawal, transfer) = (item1, item2);
                if (item1.TypeAsString == "transfer")
                {
                    (transfer, withdrawal) = (withdrawal, transfer);
                }

                return TransactionFactory.Create(transferType, FiatCurrency.Eur, transfer.TypeAsString,
                    Math.Abs(transfer.Amount), 0m, withdrawal.Time);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}