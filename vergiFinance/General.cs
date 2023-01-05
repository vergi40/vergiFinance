using Microsoft.VisualBasic;
using System.Text;
using vergiFinance.Brokers;
using vergiFinance.Functions;

namespace vergiFinance
{
    // Implement generic methods to handle any kind of transaction

    public static class General
    {
        /// <summary>
        /// Generates event collection entity, which can be used in various sales calculations and reports
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static IEventLog ReadKrakenTransactions(IReadOnlyList<string> lines)
        {
            // TODO IoC binding
            IBrokerService broker = new KrakenBroker();

            return broker.ReadTransactions(lines);
        }

        /// <summary>
        /// Use current date to calculate due date for given payment period.
        /// </summary>
        /// <returns>Print containing relevant info</returns>
        public static string CalculateDueDate(int paymentPeriodDays)
        {
            var dateCalculator = new DateCalculator();
            var dueDate = dateCalculator.CalculateDueDate(paymentPeriodDays);

            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine($"---");
            messageBuilder.AppendLine($"Current date: {DateTime.Now.ToShortDateString()}");
            messageBuilder.AppendLine($"Due date with {paymentPeriodDays} days payment period: {dueDate}");
            return messageBuilder.ToString();
        }

        /// <summary>
        /// Calculate work days for current month
        /// </summary>
        /// <returns></returns>
        public static string CalculateWorkDays()
        {
            var current = DateTime.Now;

            var calculator = new WorkDaysCalculator();
            var (workDays, publicHolidays) = calculator.CalculateWorkDaysForMonth(current.Year, current.Month);

            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine($"---");
            messageBuilder.AppendLine($"Current date: {current.ToShortDateString()}");
            messageBuilder.AppendLine($"Work days count: {workDays}. Holidays on work week: {publicHolidays}");
            foreach (var holiday in publicHolidays)
            {
                messageBuilder.AppendLine($"  Holiday on {holiday.ToShortDateString()}");
            }

            return messageBuilder.ToString();
        }
    }
}
