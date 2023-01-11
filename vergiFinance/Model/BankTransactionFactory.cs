using System.Globalization;
using vergiCommon;

namespace vergiFinance.Model
{
    internal class OpTransactionFactory
    {
        public List<IBankTransaction> Create(string filePath)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var csv = Get.ReadCsvFile(filePath);

            var result = new List<IBankTransaction>();
            var header = csv.Data[0];
            foreach (var dataRow in csv.Data.Skip(1))
            {
                result.Add(MapRowToInstance(dataRow));
            }
            return result;
        }

        private IBankTransaction MapRowToInstance(IReadOnlyList<string> row)
        {
            // Kirjauspäivä;Arvopäivä;Määrä EUROA;Laji;Selitys;Saaja/Maksaja;Saajan tilinumero ja pankin BIC;
            // Viite;Viesti;Arkistointitunnus
            var transaction = new BankTransactionModel()
            {
                RecordDate = DateOnly.Parse(row[0], new CultureInfo("fi-FI")),
                PaymentDate = DateOnly.Parse(row[1], new CultureInfo("fi-FI")),
                Amount = decimal.Parse(row[2]),
                Kind = row[3],
                RecordType = row[4],
                Recipient = row[5],
                BankAccount = row[6],
                Reference = row[7],
                Message = row[8],
                RecordId = row[9]
            };
            return transaction;
        }
    }
}
