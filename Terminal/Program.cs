using vergiCommon;
using vergiCommon.IFileInterface;
using vergiCommon.Input;
using vergiFinance;

namespace Terminal
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Write("Select utility:");

            var utils = InitializeBaseUtilities();

            var validInputs = new List<string>();
            for(int i = 0; i < utils.Count; i++)
            {
                validInputs.Add(Convert.ToString(i+1));
                var util = utils[i];
                Write($"  {i+1}): {util.description}");
            }

            Write();
            var input = Read.ReadInput(true);
            if (validInputs.Contains(input.InputAsString))
            {
                utils[Convert.ToInt32(input.InputAsString) - 1].action();
            }

            Write("Exit by pressing any key...");
            Console.ReadKey();
        }

        static List<(string description, Action action)> InitializeBaseUtilities()
        {
            // Simple "parameter" list which is used to build output for user
            // Add new (description,action) tuple to the end of list to add it for main loop
            var result = new List<(string description, Action action)>
            {
                ("Read Kraken transactions. Print sales tax report", () =>
                {
                    Write("Give input file path:");
                    var input = Read.ReadInput(false);
                    var file = FileFactory.Create(input.InputAsString);

                    var events = General.ReadTransactions(file.Lines);
                    var year = PrintYearRangeAndAskInput(events);
                    var report = events.PrintExtendedTaxReport(year);
                    Write(report);
                }),
                ("Read Kraken transactions. Print staking tax report", () =>
                {
                    Write("Give input file path:");
                    var input = Read.ReadInput(false);
                    var file = FileFactory.Create(input.InputAsString);

                    var events = General.ReadTransactions(file.Lines);
                    var year = PrintYearRangeAndAskInput(events);
                    var report = events.PrintStakingReport(year);
                    Write(report);
                }),
                ("Staking test", () =>
                {
                    Write("Analyzing 2021 ledger staking rewards...");
                    var path = Path.Combine(Constants.MyDocumentsTempLocation, "ledgers-2021.csv");
                    var file = FileFactory.Create(path);

                    var events = General.ReadTransactions(file.Lines);
                    var report = events.PrintStakingReport(2021);
                    Write(report);
                }),
            };

            return result;
        }

        private static int PrintYearRangeAndAskInput(IEventLog log)
        {
            Write("Select taxation target year:");

            var years = log.TransactionYearSpan();
            foreach (var year in years)
            {
                Write($"  {year}");
            }

            Write("");
            var input = Read.ReadInput(false);
            var inputAsInt = Convert.ToInt32(input.InputAsString);

            if (years.Contains(inputAsInt))
            {
                return inputAsInt;
            }
            else
            {
                throw new ArgumentException($"Invalid input: {input.InputAsString}");
            }
        }

        private static void Write(string message = "") => Console.WriteLine(message);
    }
}