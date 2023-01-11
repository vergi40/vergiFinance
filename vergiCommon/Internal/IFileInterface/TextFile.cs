using vergiCommon.Public;

namespace vergiCommon.Internal.IFileInterface
{
    public class TextFile : IFile
    {
        public string FilePath { get; set; }
        public string Extension { get; set; }
        
        public string Content { get; set; }
        public IReadOnlyList<string> Lines { get; set; }

        public TextFile(IFile file)
        {
            FilePath = file.FilePath;
            Extension = file.Extension.Replace(".", "");
            Content = file.Content;
            Lines = file.Lines;
        }

        public TextFile(string filePath, string[] lines)
        {
            FilePath = filePath;
            var file = new FileInfo(filePath);
            Extension = file.Extension.Replace(".", "");

            Lines = new List<string>(lines);
            Content = string.Join("", Lines);
        }

        /// <summary>
        /// Created from stream. Unknown file name and path
        /// </summary>
        public TextFile(string extension, List<string> lines)
        {
            FilePath = "";
            Lines = lines;

            Extension = extension;
            Content = string.Join("", Lines);
        }
    }
}
