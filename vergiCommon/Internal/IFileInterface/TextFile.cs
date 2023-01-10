namespace vergiCommon.Internal.IFileInterface
{
    public class TextFile : IFile
    {
        public string FilePath { get; set; }
        public string Extension { get; set; }
        
        public string Content { get; set; }
        public List<string> Lines { get; set; }


        public TextFile(string filePath, string[] lines)
        {
            FilePath = filePath;
            var file = new FileInfo(filePath);
            Extension = file.Extension;

            Lines = new List<string>(lines);
            Content = string.Join("", Lines);
        }

        public TextFile(string extension, List<string> lines)
        {
            Lines = lines;

            Extension = extension;
            Content = string.Join("", Lines);
        }
    }
}
