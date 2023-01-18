using Microsoft.VisualBasic;
using System;
using System.Text;
using vergiFinance.Brokers;
using vergiFinance.Functions;
using vergiFinance.Model;

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

        public static IReadOnlyList<IBankTransaction> ReadBankTransactions(string filePath)
        {
            var factory = new OpTransactionFactory();

            var transactions = factory.Create(filePath);
            return transactions;
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

            return CalculateWorkDaysForMonth(current.Month);
        }

        /// <summary>
        /// Calculate work days for current month
        /// </summary>
        /// <returns></returns>
        public static string CalculateWorkDaysForMonth(int month)
        {
            var year = DateTime.Now.Year;

            var calculator = new WorkDaysCalculator();
            var (workDays, publicHolidays) = calculator.CalculateWorkDaysForMonth(year, month);

            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine($"---");
            messageBuilder.AppendLine($"Given month: {month}");
            messageBuilder.AppendLine($"Work days count: {workDays}. Holidays on work week: {publicHolidays.Count}");
            foreach (var holiday in publicHolidays)
            {
                messageBuilder.AppendLine($"  Holiday on {holiday.date:d}: {holiday.name}");
            }

            return messageBuilder.ToString();
        }

        public static string GenerateSalesEstimateReport(int year, double hourlyBilling, double workHoursInDay)
        {
            var message = new StringBuilder("---");
            var calculator = new WorkDaysCalculator();

            var sum = 0.0;
            for (int i = 0; i < 12; i++)
            {
                var (workDays, _) = calculator.CalculateWorkDaysForMonth(year, i+1);
                message.AppendLine($"Month: {i + 1}. Work days: {workDays}");

                var estimate = workDays * workHoursInDay * hourlyBilling;
                message.AppendLine($"  Sales estimation: {estimate:F2}e");
                sum += estimate;
            }

            message.AppendLine("---");
            message.AppendLine($"Absolute total: {sum:F2}e");
            message.AppendLine($"Realistic (subtracting 1/12 for holiday): {sum - sum * (1 / (double)12):F2}e");

            return message.ToString();
        }
    }
}
