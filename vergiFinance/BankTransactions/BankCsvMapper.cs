using System.Globalization;

namespace vergiFinance.BankTransactions;

internal abstract class BankCsvMapper
{
    protected static readonly CultureInfo _format = new CultureInfo("fi-FI");
    public abstract IBankTransaction MapRowToInstance(IReadOnlyList<string> row);
}