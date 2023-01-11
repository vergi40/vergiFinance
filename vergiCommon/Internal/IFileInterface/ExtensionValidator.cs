namespace vergiCommon.Internal.IFileInterface;

class ExtensionValidator
{
    private HashSet<string> TextExtensions => new HashSet<string>
    {
        ".txt", ".csv", ".md", ".json", ".xml"
    };

    public bool IsTextFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension)) return false;

        return TextExtensions.Contains(extension);
    }

    public bool IsZipFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension)) return false;

        return extension.Equals(".zip");
    }
}