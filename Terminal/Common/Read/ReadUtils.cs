using System.IO.Compression;
using Terminal.Common.IFileInterface;

namespace Terminal.Common.Read
{
    public class ReadUtils
    {
        public static IFile ReadFromZipFile(string zipPath, string fileName)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                var entry = archive.GetEntry(fileName);

                return FileFactory.Create(entry.Open(), "csv");
            }
        }
    }
}
