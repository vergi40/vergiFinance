namespace vergiCommon.Public
{
    public interface IFile
    {
        string FilePath { get; }

        /// <summary>
        /// File extension without .
        /// </summary>
        string Extension { get; }
        string Content { get; }
        IReadOnlyList<string> Lines { get; }
    }
}
