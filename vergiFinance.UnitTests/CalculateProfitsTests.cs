using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiFinance.Brokers.Kraken.Operations;
using vergiFinance.FinanceFunctions;
using vergiFinance.Model;

namespace vergiFinance.UnitTests
{
    [TestFixture]
    public class CalculateProfitsTests
    {
        [Test]
        public void CalculateProfits_ShouldMatch()
        {
            var transactionList = new List<TransactionBase>();

            // Buy 100 @5, 500
            // Buy 100 @10, 1000
            // Sell 50 @10, 500. FIFO first shares (50) rose 250 -> 500 = 250 profit.
            // Sell 100 @20, 2000. FIFO (50) 250 -> 1000, (50) 500 -> 1000. = 1250 profit.
            // 50 shares left


            // Also: buy 1500, sell 2500.
            transactionList.Add(TransactionFactory.Create(TransactionType.Buy, FiatCurrency.Eur, "NOK", 100, 5, DateTime.Now));
            transactionList.Add(TransactionFactory.Create(TransactionType.Buy, FiatCurrency.Eur, "NOK", 100, 10, DateTime.Now));
            transactionList.Add(TransactionFactory.Create(TransactionType.Sell, FiatCurrency.Eur, "NOK", 50, 10, DateTime.Now));
            transactionList.Add(TransactionFactory.Create(TransactionType.Sell, FiatCurrency.Eur, "NOK", 100, 20, DateTime.Now));
            var sales = SalesFactory.ProcessSalesForYear(transactionList, DateTime.Now.Year);
            var result = sales.TotalProfitLoss();

            Assert.That(result, Is.EqualTo(1500m));
        }

        [Test]
        public void CalculateProfits_InvalidBuyList_ShouldThrow()
        {
            var transactionList = new List<TransactionBase>();

            // Buy 100 @5, 500
            // Sell 50 @10, 500. FIFO first shares (50) rose 250 -> 500 = 250 profit.
            // Sell 100 @20, 2000. FIFO (50) 250 -> 1000, tries to substract remaining 50 -> exception

            // Also: buy 1500, sell 2500.
            transactionList.Add(TransactionFactory.Create(TransactionType.Buy, FiatCurrency.Eur, "NOK", 100, 5, DateTime.Now));
            transactionList.Add(TransactionFactory.Create(TransactionType.Sell, FiatCurrency.Eur, "NOK", 50, 10, DateTime.Now));
            transactionList.Add(TransactionFactory.Create(TransactionType.Sell, FiatCurrency.Eur, "NOK", 100, 20, DateTime.Now));

            Assert.Throws<ArgumentException>(() =>
            {
                var sales = SalesFactory.ProcessSalesForYear(transactionList, DateTime.Now.Year);
                var result = sales.TotalProfitLoss();
            });
        }
    }
}
