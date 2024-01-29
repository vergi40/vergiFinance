using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicHoliday;

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

        public DateOnly CalculateDueDateOnWorkDay(int paymentPeriodDays)
        {
            var currentDate = DateTime.Now;
            var absoluteDueDate = currentDate.AddDays(paymentPeriodDays);

            var holidays = new FinlandPublicHoliday(false);
            var dueDate = holidays.NextWorkingDay(absoluteDueDate);
            
            return DateOnly.FromDateTime(dueDate);
        }
    }
}
