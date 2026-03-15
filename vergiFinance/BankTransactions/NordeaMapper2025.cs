using vergiFinance.Model;

namespace vergiFinance.BankTransactions;

internal class NordeaMapper2025 : BankCsvMapper
{
    /// <summary>
    /// Very similar to previous, new column "Viesti"
    /// </summary>
    public const string Header =
        "Kirjauspäivä;Määrä;Maksaja;Maksunsaaja;Nimi;Otsikko;Viesti;Viitenumero";
    public static IReadOnlyList<string> HeaderList => Header.Split(";");

    public override IBankTransaction MapRowToInstance(IReadOnlyList<string> row)
    {
        var amount = decimal.Parse(row[1]);
        // Column [5] Otsikko has the recipient info
        var recipient = row[5].Trim();

        var transaction = new BankTransactionModel()
        {
            Amount = amount,
            Recipient = recipient,
            RecordDate = DateTime.Parse(row[0], _format),
            PaymentDate = DateTime.Parse(row[0], _format),

            Message = row[6],
            Reference = row[7],
        };
        return transaction;
    }
}