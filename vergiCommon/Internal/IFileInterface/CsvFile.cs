using vergiCommon.Public;

namespace vergiCommon.Internal.IFileInterface
{
    internal class CsvFile : TextFile, ICsvFile
    {
        public bool HasHeaderDefinition => Headers.Any();
        public IReadOnlyList<string> Headers { get; init; }
        public IReadOnlyList<IReadOnlyList<string>> Data { get; init; }
        public string Separator { get; init; }

        public CsvFile(IFile file) : base(file)
        {
            // non-nullable fix
            Headers = new List<string>();
            Data = new List<IReadOnlyList<string>>();
            Separator = "";
        }
    }
}
