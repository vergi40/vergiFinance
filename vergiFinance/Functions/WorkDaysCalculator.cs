using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicHoliday;

namespace vergiFinance.Functions
{
    internal class WorkDaysCalculator
    {
        public (int workDays, IList<DateTime> workHolidays) CalculateWorkDaysForMonth(int year, int month)
        {
            var workDays = new List<DateTime>();
            var workHolidays = new List<DateTime>();
            var publicHolidays = new SwedenPublicHoliday().PublicHolidays(year);

            for (int i = 0; i < DateTime.DaysInMonth(year, month); i++)
            {
                var day = new DateTime(year, month, i + 1);
                if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;
                if (publicHolidays.Any(d => d.DayOfYear == day.DayOfYear))
                {
                    workHolidays.Add(day);
                    continue;
                }

                workDays.Add(day);
            }

            return (workDays.Count, workHolidays);
        }
    }
}
