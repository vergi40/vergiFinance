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
            messageBuilder.AppendLine($"Current date: {DateTime.Now}");
            messageBuilder.AppendLine($"Due date with {paymentPeriodDays} days payment period: {dueDate}");
            return messageBuilder.ToString();
        }
    }
}
