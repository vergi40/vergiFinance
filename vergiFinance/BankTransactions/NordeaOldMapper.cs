using vergiFinance.Model;

namespace vergiFinance.BankTransactions;

/// <summary>
/// 2022 and older csv
/// </summary>
internal class NordeaOldMapper : BankCsvMapper
{
    public const string Header =
        "Kirjauspäivä\tArvopäivä\tMaksupäivä\tMäärä\tSaaja/Maksaja\tTilinumero\tBIC\tTapahtuma\tViite\tMaksajan viite\tViesti";
    public static IReadOnlyList<string> HeaderList => Header.Split("\t");

    public override IBankTransaction MapRowToInstance(IReadOnlyList<string> row)
    {
        var transaction = new BankTransactionModel()
        {
            RecordDate = DateTime.Parse(row[0], _format),
            PaymentDate = DateTime.Parse(row[1], _format),
            Amount = decimal.Parse(row[3]),
            Recipient = row[4],
            BankAccount = row[5],
            Reference = row[8],
            Message = row[10]
        };
        return transaction;
    }
}