using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using vergiFinance.Model;

namespace vergiFinance.UnitTests
{
    [TestFixture]
    internal class BankTransactionTests
    {
        private OpTransactionFactory _factory;

        [SetUp]
        public void Setup()
        {
            CultureInfo.CurrentCulture = new CultureInfo("fi-FI");
            _factory = new OpTransactionFactory();
        }

        [Test]
        public void Test()
        {
            var rows = "02.09.2022;02.09.2022;-46,63;700;TILISIIRTO;aaa;FI00 0000 DABAFIHH;00000; ;1234"
                .Split(";").ToList();

            var result = _factory.MapRowToInstance(rows);
            result.Amount.ShouldBe(-46.63m);
            result.PaymentDate.ShouldBe(new DateOnly(2022, 9, 2));
        }
    }
}
