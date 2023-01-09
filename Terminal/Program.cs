using System.Globalization;
using vergiCommon;
using vergiCommon.IFileInterface;
using vergiCommon.Input;
using vergiFinance;
using vergiFinance.Model;

namespace Terminal
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Write("Select utility:");

            // Handy way for simple choice-based console to simultaneously list choices and call their delegated functions 
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
            else
            {
                Write($"Incorrect selection [{input.InputAsString}]");
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
                    Write("Give input file path (full path without \"\"):");
                    var input = Read.ReadInput(false);
                    var file = FileFactory.Create(input.InputAsString);

                    var events = General.ReadKrakenTransactions(file.Lines);
                    var year = PrintYearRangeAndAskInput(events);
                    var report = events.PrintExtendedTaxReport(year);
                    Write(report);
                }),
                ("Read Kraken transactions. Print staking tax report", () =>
                {
                    Write("Give input file path (full path without \"\"):");
                    var input = Read.ReadInput(false);
                    var file = FileFactory.Create(input.InputAsString);

                    var events = General.ReadKrakenTransactions(file.Lines);
                    var year = PrintYearRangeAndAskInput(events);
                    var report = events.PrintStakingReport(year);
                    Write(report);
                }),
                ("Staking test", () =>
                {
                    Write("Analyzing 2021 ledger staking rewards...");
                    var path = Path.Combine(Constants.MyDocumentsTempLocation, "ledgers-2021.csv");
                    var file = FileFactory.Create(path);

                    var events = General.ReadKrakenTransactions(file.Lines);
                    var report = events.PrintStakingReport(2021);
                    Write(report);
                }),
                ("Calculate due date for input", () =>
                {
                    Write("Using current date as basis. Give amount of due days:");
                    var input = Read.ReadInput(false);
                    var days = input.ToInt();

                    var report = General.CalculateDueDate(days);
                    Write(report);
                }),
                ("Calculate work day amount in current month", () =>
                {
                    Write("Using current month as basis.");

                    var report = General.CalculateWorkDays();
                    Write(report);
                }),
                ("Calculate work day amount in given month", () =>
                {
                    Write("Please input month: ");
                    var input = Read.ReadInput(false);

                    var report = General.CalculateWorkDaysForMonth(input.ToInt());
                    Write(report);
                }),
                ("Print work days and work holidays for full year", () =>
                {
                    Write("Work days and work holidays:");

                    for(int i = 0; i < 12; i++)
                    {
                        var report = General.CalculateWorkDaysForMonth(i+1);
                        Write(report);
                    }
                }),
                ("Calculate projected sales for given year", () =>
                {
                    Write("Input year, hourly billing and work hours per day");
                    Write("Input syntax should be YEAR;HOURBILL;HPERDAY. Example: 2023;80;7,5");

                    var input = Read.ReadInput(false);
                    var array = input.InputAsString.Split(';').ToList();

                    var (year, hourly, lengthStr) = (int.Parse(array[0]), double.Parse(array[1]), array[2]);

                    var lengthStr2 = lengthStr.Replace('.', ',');
                    var length = double.Parse(lengthStr2);
                    var report = General.GenerateSalesEstimateReport(year, hourly, length);
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