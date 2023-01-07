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
        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// workDays: Work day count
        /// workHolidays: holidays on work week, with holiday names
        /// </returns>
        public (int workDays, IList<(DateTime date, string name)> workHolidays) CalculateWorkDaysForMonth(int year, int month)
        {
            var workDays = new List<DateTime>();
            var workHolidays = new List<(DateTime, string)>();
            var calendar = new FinlandPublicHoliday(true);
            var publicHolidayNames = calendar.PublicHolidayNames(year);

            for (int i = 0; i < DateTime.DaysInMonth(year, month); i++)
            {
                var day = new DateTime(year, month, i + 1);
                if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;
                if (publicHolidayNames.ContainsKey(day))
                {
                    var name = publicHolidayNames[day];
                    workHolidays.Add((day, name));
                    continue;
                }

                workDays.Add(day);
            }

            return (workDays.Count, workHolidays);
        }
    }
}
