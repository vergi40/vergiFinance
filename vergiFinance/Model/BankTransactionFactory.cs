using System.Globalization;
using vergiCommon;

namespace vergiFinance.Model
{
    internal class OpTransactionFactory
    {
        private static IFormatProvider _format = new CultureInfo("fi-FI");

        public List<IBankTransaction> Create(string filePath)
        {
            CultureInfo.CurrentCulture = new CultureInfo("fi-FI");
            var csv = Get.ReadCsvFile(filePath);

            var result = new List<IBankTransaction>();
            var header = csv.Data[0];
            foreach (var dataRow in csv.Data.Skip(1))
            {
                result.Add(MapRowToInstance(dataRow));
            }
            return result;
        }

        internal IBankTransaction MapRowToInstance(IReadOnlyList<string> row)
        {
            // Kirjauspäivä;Arvopäivä;Määrä EUROA;Laji;Selitys;Saaja/Maksaja;Saajan tilinumero ja pankin BIC;
            // Viite;Viesti;Arkistointitunnus
            var transaction = new BankTransactionModel()
            {
                RecordDate = DateTime.Parse(row[0], _format),
                PaymentDate = DateTime.Parse(row[1], _format),
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
