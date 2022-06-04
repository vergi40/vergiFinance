using System.IO.Compression;
using vergiCommon.IFileInterface;

namespace vergiCommon.File
{
    public class ReadUtils
    {
        public static IFile ReadFromZipFile(string zipPath, string fileName)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                var entry = archive.GetEntry(fileName);

                // TODO read all files/extensions and select best
                return FileFactory.Create(entry.Open(), "csv");
            }
        }
    }
}
