using System.Globalization;

namespace vergiFinance.Brokers.Kraken;

/// <summary>
/// Lists each property that can be read from kraken csv ledger print.
/// </summary>
public class RawTransaction
{
    /// <summary>
    /// Unique
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Linked transactions have same ref id 
    /// </summary>
    public string ReferenceId { get; set; } = "";

    public DateTime Time { get; set; }
    public string TypeAsString { get; set; } = "";
    public TransactionType Type { get; set; }
    public string SubType { get; set; } = "";

    /// <summary>
    /// Usually "currency"
    /// </summary>
    public string AssetClass { get; set; } = "";

    /// <summary>
    /// ZEUR, XETH, XTRX...
    /// </summary>
    public string Asset { get; set; } = "";


    public decimal Amount { get; set; }
    public decimal Balance { get; set; }

    /// <summary>
    /// Fee in given asset class
    /// </summary>
    public decimal Fee { get; set; }

    public override string ToString()
    {
        var info = $"Asset: {Asset}, transaction: {TypeAsString}, amount: {Amount}, fee: {Fee}";
        return info;
    }

    public static RawTransaction Parse(string line)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        line = line.Replace("\"", "");
        var columns = line.Split(",");

        DateTime.TryParse(columns[2], out var dateTime);
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