using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace vergiFinance.Persistence
{
    public class DatabaseLite : IDatabase
    {
        private readonly string _dbName;

        public DatabaseLite(string dbName = "staking.db")
        {
            _dbName = dbName;
        }

        public bool TryLoadSingleStakingData(string ticker, DateTime tradeDate, out StakingDto data)
        {
            data = new StakingDto();
            using (var db = new LiteDatabase(_dbName))
            {
                var collection = db.GetCollection<StakingDto>("staking");

                var results = collection.Query()
                    .Where(x => x.Ticker == ticker && x.TradeDate.Date.Equals(tradeDate.Date))
                    .ToList();

                if (results == null || results.Count == 0) return false;
                if (results.Count > 1)
                {
                    throw new ArgumentException($"More than one entry found for [{ticker}, {tradeDate.Date}]");
                }

                data = results.Single();
                return true;
            }
        }

        public bool TryLoadStakingData(List<(string ticker, DateTime tradeDate)> pars, out StakingDto data)
        {
            throw new NotImplementedException();
        }

        public void SaveSingleStakingData(StakingDto dto)
        {
            using (var db = new LiteDatabase(_dbName))
            {
                var collection = db.GetCollection<StakingDto>("staking");
                collection.Upsert(dto);
            }
        }

        public void SaveStakingData(List<StakingDto> dtos)
        {
            throw new NotImplementedException();
        }

        public void ClearTable(string tableName = "staking")
        {
            using (var db = new LiteDatabase(_dbName))
            {
                db.DropCollection(tableName);
            }
        }
    }
}
