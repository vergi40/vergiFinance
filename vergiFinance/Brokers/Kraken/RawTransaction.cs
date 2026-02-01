using vergiFinance.Model;

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

    public string DebugOriginalCsvLine { get; init; }

    public override string ToString()
    {
        var info = $"Asset: {Asset}, transaction: {TypeAsString}, amount: {Amount}, fee: {Fee}";
        return info;
    }
}