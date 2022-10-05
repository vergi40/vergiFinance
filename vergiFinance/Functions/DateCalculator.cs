using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiFinance.Functions
{
    internal class DateCalculator
    {
        public DateOnly CalculateDueDate(int paymentPeriodDays)
        {
            var currentDate = DateTime.Now;
            var dueDate = currentDate.AddDays(paymentPeriodDays);

            return DateOnly.FromDateTime(dueDate);
        }
    }
}
