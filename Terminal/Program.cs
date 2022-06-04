using Terminal.Common.IFileInterface;
using vergiFinance;

namespace Terminal
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Select utility:");

            var utils = InitializeBaseUtilities();

            var validInputs = new List<string>();
            for(int i = 0; i < utils.Count; i++)
            {
                validInputs.Add(Convert.ToString(i+1));
                var util = utils[i];
                Console.WriteLine($"  {i+1}): {util.description}");
            }

            var input = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine();
            if (validInputs.Contains(input))
            {
                utils[Convert.ToInt32(input) - 1].action();
            }

            Console.WriteLine("Exit by pressing any key...");
            Console.ReadKey();
        }

        static List<(string description, Action action)> InitializeBaseUtilities()
        {
            var result = new List<(string description, Action action)>
            {
                ("Read Kraken transactions", () =>
                {
                    Console.WriteLine("Give input file path:");
                    var filePath = Console.ReadLine();
                    var file = FileFactory.Create(filePath);

                    var events = General.ReadTransactions(file.Lines);
                    var report = events.PrintExtendedTaxReport(2021);
                    Console.WriteLine(report);
                })
            };

            return result;
        }
    }
}