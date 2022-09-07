using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;
using vergiFinance.Persistence;

namespace vergiFinance.UnitTests
{
    [TestFixture]
    internal class PersistenceTests
    {
        private Persistence.Persistence _dbInstance;

        [SetUp]
        public void Setup()
        {
            _dbInstance = new Persistence.Persistence(new DatabaseLite("test.db"));
        }

        [TearDown]
        public void Cleanup()
        {
            _dbInstance.ClearTable();
        }

        [Test]
        public void SaveAndLoad()
        {
            var dto = new StakingDto()
            {
                DayUnitPrice = 666, BrokerId = "test", Ticker = "test", TradeDate = new DateTime(2020, 1, 1)
            };

            _dbInstance.SaveSingleStakingData(dto);

            Assert.IsTrue(_dbInstance.TryLoadSingleStakingData(dto.Ticker, dto.TradeDate, out var result));
            Assert.AreEqual(dto.DayUnitPrice, result.DayUnitPrice);

        }

    }
}
