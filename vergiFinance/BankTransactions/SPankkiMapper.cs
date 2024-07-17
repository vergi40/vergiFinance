using vergiFinance.Model;

namespace vergiFinance.BankTransactions;

internal class SPankkiMapper : BankCsvMapper
{
    // Avaa tili -> valitse aikaväli -> vie csv

    public const string Header =
        "Kirjauspäivä;Maksupäivä;Summa;Tapahtumalaji;Maksaja;Saajan nimi;Saajan tilinumero;Saajan BIC-tunnus;Viitenumero";
    public static IReadOnlyList<string> HeaderList => Header.Split(";");

    public override IBankTransaction MapRowToInstance(IReadOnlyList<string> row)
    {
        var amount = decimal.Parse(row[2]);
        // 3 & 4 = maksaja/saaja.
        var recipient = row[4];
        if (amount <= 0) recipient = row[5];

        var transaction = new BankTransactionModel()
        {
            RecordDate = DateTime.Parse(row[0], _format),
            PaymentDate = DateTime.Parse(row[1], _format),
            Amount = amount,
            Kind = row[3],
            Recipient = recipient,

            // Saajan tilinumero;Saajan BIC-tunnus
            BankAccount = $"{row[6]};{row[7]}",
            Reference = row[8],
            Message = row[9],
            RecordId = row[10]
        };
        return transaction;
    }
}