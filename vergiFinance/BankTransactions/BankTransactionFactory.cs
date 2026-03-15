using System.Globalization;
using vergiCommon;
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

            BankCsvMapper mapper = SolveMapper(header);

            foreach (var dataRow in csv.Data.Skip(1))
            {
                // Some providers wrap header items with quotes like "Kirjauspäivä";"Arvopäivä";
                var dataRow2 = RemoveHyphens(dataRow);
                result.Add(mapper.MapRowToInstance(dataRow2));
            }
            return result;
        }

        internal BankCsvMapper SolveMapper(IReadOnlyList<string> header)
        {
            // Some providers wrap header items with quotes like "Kirjauspäivä";"Arvopäivä";
            header = RemoveHyphens(header);
            
            BankCsvMapper mapper;
            if (ValidateHeaderToItems(header, OpMapper.HeaderList))
            {
                mapper = new OpMapper();
            }
            else if (ValidateHeaderToItems(header, OpPersonalMapper.HeaderList))
            {
                mapper = new OpPersonalMapper();
            }
            else if (ValidateHeaderToItems(header, SPankkiMapper.HeaderList))
            {
                mapper = new SPankkiMapper();
            }
            else if (ValidateHeaderToItems(header, NordeaMapper2025.HeaderList))
            {
                mapper = new NordeaMapper2025();
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

            return mapper;
        }

        private IReadOnlyList<string> RemoveHyphens(IReadOnlyList<string> items)
        {
            return items.Select(i => i.Trim('"')).ToList();
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
}
