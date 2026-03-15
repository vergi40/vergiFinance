using vergiFinance.Model;

namespace vergiFinance.BankTransactions;

internal class OpPersonalMapper : BankCsvMapper
{
    public const string Header =
        "Kirjauspäivä;Arvopäivä;Määrä EUROA;Laji;Selitys;Saaja/Maksaja;Saajan tilinumero;Saajan pankin BIC;Viite;Viesti;Arkistointitunnus";

    public static IReadOnlyList<string> HeaderList => Header.Split(";");

    public override IBankTransaction MapRowToInstance(IReadOnlyList<string> row)
    {
        var transaction = new BankTransactionModel()
        {
            RecordDate = DateTime.Parse(row[0], _format),
            PaymentDate = DateTime.Parse(row[1], _format),
            Amount = decimal.Parse(row[2]),
            Kind = row[3],
            RecordType = row[4],
            Recipient = row[5],
            BankAccount = row[6],
            Reference = row[8],
            Message = row[9],
            RecordId = row[10]
        };
        return transaction;
    }
}