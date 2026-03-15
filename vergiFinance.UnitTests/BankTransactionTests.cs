using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using vergiFinance.BankTransactions;
using vergiFinance.Model;
// ReSharper disable StringLiteralTypo

namespace vergiFinance.UnitTests
{
    [TestFixture]
    internal class BankTransactionTests
    {
        private BankTransactionFactory _transactionFactory;

        [SetUp]
        public void Setup()
        {
            CultureInfo.CurrentCulture = new CultureInfo("fi-FI");
            _transactionFactory = new BankTransactionFactory();
        }

        [Test]
        public void SolveMapper_Nordea2025()
        {
            var header = "Kirjauspäivä;Määrä;Maksaja;Maksunsaaja;Nimi;Otsikko;Viesti;Viitenumero;Saldo;Valuutta;";
            var list = header.Split(";").ToList();
            var result = _transactionFactory.SolveMapper(list);

            result.ShouldBeOfType<NordeaMapper2025>();
        }

        [Test]
        public void SolveMapper_Nordea2024()
        {
            var header = "Kirjauspäivä;Määrä;Maksaja;Maksunsaaja;Nimi;Otsikko;Viitenumero;Saldo;Valuutta;";
            var list = header.Split(";").ToList();
            var result = _transactionFactory.SolveMapper(list);

            result.ShouldBeOfType<NordeaMapper>();
        }

        [Test]
        public void SolveMapper_Nordea2023()
        {
            var header = "Kirjauspäivä;Määrä;Maksaja;Maksunsaaja;Nimi;Otsikko;Viitenumero;Valuutta;";
            var list = header.Split(";").ToList();
            var result = _transactionFactory.SolveMapper(list);

            result.ShouldBeOfType<NordeaMapper>();
        }

        [Test]
        public void SolveMapper_OpBusiness2026()
        {
            var header = "Kirjauspäivä;Arvopäivä;Määrä EUROA;Laji;Selitys;Saaja/Maksaja;Saajan tilinumero ja pankin BIC;Viite;Viesti;Arkistointitunnus";
            var list = header.Split(";").ToList();
            var result = _transactionFactory.SolveMapper(list);

            result.ShouldBeOfType<OpMapper>();
        }

        [Test]
        public void SolveMapper_Op2025_Raw()
        {
            var header = "\"Kirjauspäivä\";\"Arvopäivä\";\"Määrä EUROA\";\"Laji\";\"Selitys\";\"Saaja/Maksaja\";" +
                         "\"Saajan tilinumero\";\"Saajan pankin BIC\";\"Viite\";\"Viesti\";\"Arkistointitunnus\"";
            var list = header.Split(";").ToList();
            var result = _transactionFactory.SolveMapper(list);

            result.ShouldBeOfType<OpPersonalMapper>();
        }

        [Test]
        public void SolveMapper_Op2025_NoQuotes()
        {
            var header = "Kirjauspäivä;Arvopäivä;Määrä EUROA;Laji;Selitys;Saaja/Maksaja;Saajan tilinumero;Saajan pankin BIC;Viite;Viesti;Arkistointitunnus";
            var list = header.Split(";").ToList();
            var result = _transactionFactory.SolveMapper(list);

            result.ShouldBeOfType<OpPersonalMapper>();
        }
    }
}
