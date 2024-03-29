﻿using vergiCommon;
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
            var input = Get.ReadInput(true);
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
                    var input = Get.ReadInput(false);
                    var file = Get.ReadFile(input.InputAsString);

                    var events = Api.ReadKrakenTransactions(file.Lines);
                    var year = PrintYearRangeAndAskInput(events);
                    var report = events.PrintExtendedTaxReport(year);
                    Write(report);
                }),
                ("Read Kraken transactions. Print staking tax report", () =>
                {
                    Write("Give input file path (full path without \"\"):");
                    var input = Get.ReadInput(false);
                    var file = Get.ReadFile(input.InputAsString);

                    var events = Api.ReadKrakenTransactions(file.Lines);
                    var year = PrintYearRangeAndAskInput(events);
                    var report = events.PrintStakingReport(year);
                    Write(report);
                }),
                ("Staking test", () =>
                {
                    Write("Analyzing 2021 ledger staking rewards...");
                    var path = Path.Combine(Constants.MyDocumentsTempLocation, "ledgers-2021.csv");
                    var file = Get.ReadFile(path);

                    var events = Api.ReadKrakenTransactions(file.Lines);
                    var report = events.PrintStakingReport(2021);
                    Write(report);
                }),
                ("Calculate due date for input", () =>
                {
                    Write("Using current date as basis. Give amount of due days:");
                    var input = Get.ReadInput(false);
                    var days = input.ToInt();

                    var report = Api.CalculateDueDate(days);
                    Write(report);
                }),
                ("Calculate work day amount in current month", () =>
                {
                    Write("Using current month as basis.");

                    var report = Api.CalculateWorkDays();
                    Write(report);
                }),
                ("Calculate work day amount in given month", () =>
                {
                    Write("Please input month: ");
                    var input = Get.ReadInput(false);

                    var report = Api.CalculateWorkDaysForMonth(input.ToInt());
                    Write(report);
                }),
                ("Print work days and work holidays for full year", () =>
                {
                    Write("Work days and work holidays:");

                    for(int i = 0; i < 12; i++)
                    {
                        var report = Api.CalculateWorkDaysForMonth(i+1);
                        Write(report);
                    }
                }),
                ("Calculate projected sales for given year", () =>
                {
                    Write("Input year, hourly billing and work hours per day");
                    Write("Input syntax should be YEAR;HOURBILL;HPERDAY. Example: 2023;80;7,5");

                    var input = Get.ReadInput(false);
                    var array = input.InputAsString.Split(';').ToList();

                    var (year, hourly, lengthStr) = (int.Parse(array[0]), double.Parse(array[1]), array[2]);

                    var lengthStr2 = lengthStr.Replace('.', ',');
                    var length = double.Parse(lengthStr2);
                    var report = Api.GenerateSalesEstimateReport(year, hourly, length);
                    Write(report);
                }),
                ("DEBUG Read and print bank transactions", () =>
                {
                    Write("Give input file path:");
                    var input = Get.ReadInput(false);

                    var transactions = Api.ReadBankTransactions(input.InputAsString);
                    foreach (var t in transactions)
                    {
                        // TODO from/to based on positive/negative amount
                        Console.WriteLine($"{t.RecordDate:d}: {t.Amount:F2} to ({t.Recipient}, {t.BankAccount})");
                    }
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
            var input = Get.ReadInput(false);
            return input.ToInt();
        }

        private static void Write(string message = "") => Console.WriteLine(message);
    }
}