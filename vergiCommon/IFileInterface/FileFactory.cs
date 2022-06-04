namespace vergiCommon.IFileInterface
{
    public class FileFactory
    {
        public static IFile Create(string filePath)
        {
            // TODO open zip files


            var data = System.IO.File.ReadAllLines(filePath);

            // If text file
            var file = new TextFile(filePath, data);
            return file;
        }

        public static IFile Create(Stream stream, string extension)
        {
            var lines = new List<string>();
            using (StreamReader reader = new StreamReader(stream))
            {
                while (reader.Peek() >= 0)
                {
                    lines.Add(reader.ReadLine());
                }
            }

            // If text file
            var file = new TextFile(extension, lines);
            return file;
        }
    }
}
