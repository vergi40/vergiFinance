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
                return FileFactory.Create(entry.Open(), "csv");
            }
        }

        public static IFile ReadFirstFromZipFile(string zipPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                var entry = archive.Entries.FirstOrDefault();
                var extension = Path.GetExtension(entry.FullName);
                extension = extension.Replace(".", "");

                return FileFactory.Create(entry.Open(), extension);
            }
        }
    }
}
