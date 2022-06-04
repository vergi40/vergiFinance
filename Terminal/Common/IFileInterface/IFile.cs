namespace Terminal.Common.IFileInterface
{
    public interface IFile
    {
        string FilePath { get; set; }
        string Extension { get; set; }
        string Content { get; set; }
        List<string> Lines { get; set; }
    }
}
