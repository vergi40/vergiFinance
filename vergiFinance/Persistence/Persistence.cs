using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace vergiFinance.Persistence
{
    public interface IDatabase
    {
        /// <summary>
        /// </summary>
        /// <param name="ticker"></param>
        /// <param name="tradeDate"></param>
        /// <param name="data">The data found</param>
        /// <returns>True if found</returns>
        bool TryLoadSingleStakingData(string ticker, DateTime tradeDate, out StakingDto data);

        /// <summary>
        /// Insert or update given data
        /// </summary>
        /// <param name="dto"></param>
        void SaveSingleStakingData(StakingDto dto);

        /// <summary>
        /// Clear contents of table
        /// </summary>
        /// <param name="tableName"></param>
        void ClearTable(string tableName = "staking");
    }

    public class Persistence
    {
        private readonly string _dbName;
        private readonly IDatabase _db;

        public Persistence(IDatabase db, string dbName = "staking.db")
        {
            _db = db;
            _dbName = dbName;
        }

        public bool TryLoadSingleStakingData(string ticker, DateTime tradeDate, out StakingDto data)
        {
            return _db.TryLoadSingleStakingData(ticker, tradeDate, out data);
        }
        
        public void SaveSingleStakingData(StakingDto dto)
        {
            _db.SaveSingleStakingData(dto);
        }

        public void ClearTable(string tableName = "staking")
        {
            _db.ClearTable(tableName);
        }
    }
}
