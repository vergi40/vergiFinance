using vergiFinance.Model;

namespace vergiFinance.BankTransactions;

internal class NordeaMapper : BankCsvMapper
{
    public const string Header =
        "Kirjauspäivä;Määrä;Maksaja;Maksunsaaja;Nimi;Otsikko;Viitenumero";
    public static IReadOnlyList<string> HeaderList => Header.Split(";");

    public override IBankTransaction MapRowToInstance(IReadOnlyList<string> row)
    {
        var transaction = new BankTransactionModel()
        {
            RecordDate = DateTime.Parse(row[0], _format),
            PaymentDate = DateTime.Parse(row[0], _format),
            Amount = decimal.Parse(row[1]),
            Recipient = row[5],
            Reference = row[6],
        };
        return transaction;
    }
}