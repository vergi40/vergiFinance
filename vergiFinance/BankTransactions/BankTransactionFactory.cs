﻿using System.Globalization;
using vergiCommon;
using vergiFinance.Model;
// ReSharper disable CommentTypo

namespace vergiFinance.BankTransactions
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
