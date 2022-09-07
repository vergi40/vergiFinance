﻿using System;

namespace vergiFinance
{
    /// <summary>
    /// Collection of most basic properties about single transaction (buy/sell/deposit/dividend etc)
    /// Using nordnet transaction properties as example
    /// </summary>
    public class TransactionBase
    {
        /// <summary>
        /// Nordnet: Nro
        /// Kraken: Id
        /// </summary>
        public string Id { get; set; } = "";
        public TransactionType Type { get; set; }

        /// <summary>
        /// Every transaction is based on one fiat currency
        /// </summary>
        public FiatCurrency FiatCurrency { get; set; }

        /// <summary>
        /// NOK, NVDA, ETH, 
        /// </summary>
        public string Ticker { get; set; } = "";
        /// <summary>
        /// Nokia, NVidia, Ethereum
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Amount of units in the transaction. Non-negative.
        /// Kpl määrä. Esim 8 kpl osakkeita. 0.001 kpl kryptoa
        /// </summary>
        public decimal AssetAmount { get; set; }

        /// <summary>
        /// Price of a singe unit (single share, single coin) in fiat currency. Non-negative.
        /// Kurssi, osakekurssi
        /// </summary>
        public decimal AssetUnitPrice { get; set; }

        /// <summary>
        /// Transaction total price in fiat currency. Non-negative.
        /// Kauppahinta
        /// </summary>
        public decimal TotalPrice => AssetAmount * AssetUnitPrice;

        /// <summary>
        /// Välityspalkkio
        /// </summary>
        public decimal Fee { get; set; }

        /// <summary>
        /// Local currency compared to fiat currency (e.g. USD / EUR).
        /// All prices are converted to fiat currency, if originally in other currency.
        /// </summary>
        public decimal? ExchangeRate { get; set; } = null;

        /// <summary>
        /// Exchange name. HEL, NY 
        /// </summary>
        public string Market { get; set; } = "";

        /// <summary>
        /// When trade occured
        /// </summary>
        public DateTime TradeDate { get; set; }

        /// <summary>
        /// When the money was transferred from buyer to seller. Stock exchanges
        /// </summary>
        [Obsolete("Not implemented")]
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Ticker unit price in midday of <see cref="TradeDate"/>
        /// </summary>
        public decimal DayUnitPrice { get; set; }

        public override string ToString()
        {
            return $"{Type.ToString()}: {Ticker} -> {TotalPrice:F2} {FiatCurrency}";
        }

        public string ToKrakenTransactionString(char fiatType = 'e')
        {
            var totalPrice = $"{TotalPrice:F2}{fiatType}";
            var assetAmount = $"{AssetAmount:G8}";
            var assetUnitPrice = $"{AssetUnitPrice:F2}";
            if (AssetUnitPrice < 1) assetUnitPrice = $"{AssetUnitPrice:F4}";
            var message = $"{TradeDate.Date:dd/MM/yyyy} {Type.ToString(),-5}{totalPrice,-10} amount {assetAmount,-11} unit price {assetUnitPrice}{fiatType}";
            return message;
        }

        public string ToKrakenDividendString(char fiatType = 'e')
        {
            var assetAmount = $"{AssetAmount:G8}";
            var ticker = $"{Ticker}:";
            var rewardAmountText = $"reward amount {assetAmount}";
            var unitPrice = $"{DayUnitPrice:G8}";
            var priceText = $"midday price {unitPrice}";
            var message = $"{TradeDate.Date:dd/MM/yyyy} {ticker,-8} {rewardAmountText,-30} {priceText}";
            return message;
        }

        public TransactionBase ShallowCopy()
        {
            return (TransactionBase) MemberwiseClone();
        }

        public TransactionBase DeepCopy()
        {
            var other = ShallowCopy();
            // TODO maybe change this class to immutable?

            return other;
        }
    }
}