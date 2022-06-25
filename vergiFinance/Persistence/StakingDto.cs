using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiFinance.Persistence
{
    public class StakingDto
    {
        //public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Nordnet: Nro
        /// Kraken: Id
        /// </summary>
        public string BrokerId { get; set; } = "";

        /// <summary>
        /// NOK, NVDA, ETH, 
        /// </summary>
        public string Ticker { get; set; } = "";
        
        /// <summary>
        /// When trade occured
        /// </summary>
        public DateTime TradeDate { get; set; }
        
        /// <summary>
        /// Ticker unit price in midday of <see cref="TradeDate"/>
        /// </summary>
        public decimal DayUnitPrice { get; set; }
    }
}
