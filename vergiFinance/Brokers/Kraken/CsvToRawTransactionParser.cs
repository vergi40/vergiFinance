using System.Globalization;

namespace vergiFinance.Brokers.Kraken;

/// <summary>
/// Convert Kraken csv line to RawTransaction
/// </summary>
public static class CsvToRawTransactionParser
{
    /// <summary>
    /// Format from Kraken is based on what year the ledger is downloaded, not on what year it is based on.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static RawTransaction Parse(string line)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        line = line.Replace("\"", "");
        var columns = line.Split(",");

        // Pre-2025 "txid","refid","time","type","subtype","aclass","asset","amount","fee","balance"
        // 2025     "txid","refid","time","type","subtype","aclass","asset","wallet","amount","fee","balance"
        // 2026     "txid","refid","time","type","subtype","aclass","subclass","asset","wallet","amount","fee","balance"

        if (columns.Length == 10)
        {
            return ParsePre2025(line);
        }
        else if (columns.Length == 11)
        {
            return Parse2025(line);
        }
        else if (columns.Length == 12)
        {
            return Parse2026(line);
        }
        else
        {
            throw new NotImplementedException($"Unrecognized csv format. Header has {columns.Length} columns." +
                                              $"{Environment.NewLine}Header: {line}");
        }
    }

    internal static RawTransaction Parse2026(string line)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        line = line.Replace("\"", "");
        var columns = line.Split(",");

        // 2025     "txid","refid","time","type","subtype","aclass","asset",   "wallet","amount","fee",   "balance"
        // 2026     "txid","refid","time","type","subtype","aclass","subclass","asset", "wallet","amount","fee","balance"
        // "tid1","rid1","2025-07-12 09:55:49","earn","autoallocation","currency","crypto","ADA","spot / main",-0.02186300,0,0.00000000
        // "tid2","rid1","2025-07-12 09:55:49","earn","autoallocation","currency","crypto","ADA","earn / liquid",0.02186300,0,0.02186300

        if (!DateTime.TryParse(columns[2], out var dateTime))
        {
            throw new ArgumentException($"Failed to parse datetime {columns[2]}");
        }
        decimal.TryParse(columns[9], out var amount);
        decimal.TryParse(columns[10], out var fee);
        decimal.TryParse(columns[11], out var balance);

        var result = new RawTransaction()
        {
            Id = columns[0],
            ReferenceId = columns[1],
            Time = dateTime,
            TypeAsString = columns[3],
            SubType = columns[4],
            AssetClass = columns[5],
            Asset = columns[7],
            Amount = amount,
            Fee = fee,
            Balance = balance,
            DebugOriginalCsvLine = line
        };

        return result;
    }

    internal static RawTransaction Parse2025(string line)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        line = line.Replace("\"", "");
        var columns = line.Split(",");

        // 2025     "txid","refid","time","type","subtype","aclass","asset","wallet","amount","fee","balance"
        // 2026     "txid","refid","time","type","subtype","aclass","subclass","asset","wallet","amount","fee","balance"

        if (!DateTime.TryParse(columns[2], out var dateTime))
        {
            throw new ArgumentException($"Failed to parse datetime {columns[2]}");
        }
        decimal.TryParse(columns[8], out var amount);
        decimal.TryParse(columns[9], out var fee);
        decimal.TryParse(columns[10], out var balance);

        var result = new RawTransaction()
        {
            Id = columns[0],
            ReferenceId = columns[1],
            Time = dateTime,
            TypeAsString = columns[3],
            SubType = columns[4],
            AssetClass = columns[5],
            Asset = columns[6],
            Amount = amount,
            Fee = fee,
            Balance = balance,
            DebugOriginalCsvLine = line
        };

        return result;
    }

    internal static RawTransaction ParsePre2025(string line)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        line = line.Replace("\"", "");
        var columns = line.Split(",");

        // Pre-2025 "txid","refid","time","type","subtype","aclass","asset","amount","fee","balance"
        // 2025     "txid","refid","time","type","subtype","aclass","asset","wallet","amount","fee","balance"
        if (!DateTime.TryParse(columns[2], out var dateTime))
        {
            throw new ArgumentException($"Failed to parse datetime {columns[2]}");
        }
        decimal.TryParse(columns[7], out var amount);
        decimal.TryParse(columns[8], out var fee);
        decimal.TryParse(columns[9], out var balance);

        var result = new RawTransaction()
        {
            Id = columns[0],
            ReferenceId = columns[1],
            Time = dateTime,
            TypeAsString = columns[3],
            SubType = columns[4],
            AssetClass = columns[5],
            Asset = columns[6],
            Amount = amount,
            Fee = fee,
            Balance = balance,
        };

        return result;
    }
}