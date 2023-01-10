using System.IO.Compression;
using vergiCommon.IFileInterface;

namespace vergiCommon.File
{
    internal class ReadUtils
    {
        public static List<string> ReadFileNamesFromZipFile(string zipPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                return archive.Entries.Select(e => e.Name).ToList();
            }
        }

        public static IFile ReadFromZipFile(string zipPath, string fileName)
        {
            var extension = Path.GetExtension(fileName);
            extension = extension.Replace(".", "");

            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                var entry = archive.GetEntry(fileName);
                return new FileFactory().Create(entry.Open(), extension);
            }
        }

        public static IFile ReadFirstFromZipFile(string zipPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                var entry = archive.Entries.FirstOrDefault();
                var extension = Path.GetExtension(entry.FullName);
                extension = extension.Replace(".", "");

                return new FileFactory().Create(entry.Open(), extension);
            }
        }
    }
}
