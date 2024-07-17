using System.Globalization;
using vergiCommon;
// ReSharper disable CommentTypo

namespace vergiFinance.Model
{
    internal class BankTransactionFactory
    {
        private static readonly CultureInfo _format = new CultureInfo("fi-FI");
        public async Task<List<IBankTransaction>> Create(string filePath)
        {
            CultureInfo.CurrentCulture = _format;
            var csv = await Get.ReadCsvFileAsync(filePath);

            var result = new List<IBankTransaction>();
            var header = csv.Data[0];

            BankCsvMapper mapper;
            if (ValidateHeaderToItems(header, OpMapper.HeaderList))
            {
                mapper = new OpMapper();
            }
            else if (ValidateHeaderToItems(header, SPankkiMapper.HeaderList))
            {
                mapper = new SPankkiMapper();
            }
            else if (ValidateHeaderToItems(header, NordeaMapper.HeaderList))
            {
                mapper = new NordeaMapper();
            }
            else if (ValidateHeaderToItems(header, NordeaOldMapper.HeaderList))
            {
                mapper = new NordeaOldMapper();
            }
            else
            {
                throw new NotImplementedException($"Bank type for csv header type not implemented yet. [{string.Join(";", header)}]");
            }

            foreach (var dataRow in csv.Data.Skip(1))
            {
                result.Add(mapper.MapRowToInstance(dataRow));
            }
            return result;
        }

        private static bool ValidateHeaderToItems(IReadOnlyList<string> headerList, IReadOnlyList<string> definitionList)
        {
            for (int i = 0; i < definitionList.Count; i++)
            {
                // Problems with ä ö å
                if (!string.Equals(headerList[i], definitionList[i], StringComparison.CurrentCulture))
                {
                    return false;
                }
            }

            return true;
        }
    }

    internal abstract class BankCsvMapper
    {
        protected static readonly CultureInfo _format = new CultureInfo("fi-FI");
        public abstract IBankTransaction MapRowToInstance(IReadOnlyList<string> row);
    }

    internal class OpMapper : BankCsvMapper
    {
        public const string Header =
            "Kirjauspäivä;Arvopäivä;Määrä EUROA;Laji;Selitys;Saaja/Maksaja;Saajan tilinumero ja pankin BIC;Viite";

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
                Reference = row[7],
                Message = row[8],
                RecordId = row[9]
            };
            return transaction;
        }
    }

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
