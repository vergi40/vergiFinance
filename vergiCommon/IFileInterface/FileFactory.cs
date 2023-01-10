using System.Net.Mime;
using FileTypeChecker;
using vergiCommon.File;

namespace vergiCommon.IFileInterface
{
    internal class FileFactory
    {
        public IFile Create(string filePath, bool trustFileExtension = true)
        {
            if (!trustFileExtension)
            {
                // https://github.com/AJMitev/FileTypeChecker
                // Throws if unrecognizable
                IsFileRecognizable(filePath);
            }

            // Compare extension to known types
            var validator = new ExtensionValidator();
            if (validator.IsTextFile(filePath))
            {
                var data = System.IO.File.ReadAllLines(filePath);

                // If text file
                return new TextFile(filePath, data);
            }

            if (validator.IsZipFile(filePath))
            {
                return ReadUtils.ReadFirstFromZipFile(filePath);
            }

            throw new NotImplementedException("File type reading not implemented yet");
        }

        private bool IsFileRecognizable(string filePath)
        {
            using (var fileStream = System.IO.File.OpenRead(filePath))
            {
                var isRecognizable = FileTypeValidator.IsTypeRecognizable(fileStream);
                if (!isRecognizable)
                {
                    // 
                    throw new ArgumentException($"Unknown file content in path: ${filePath}");
                }

                if (FileTypeValidator.IsImage(fileStream))
                {
                    throw new ArgumentException("Image types not supported");
                }

                return true;
            }
        }

        public IFile Create(Stream stream, string extension)
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

    class ExtensionValidator
    {
        private HashSet<string> TextExtensions => new HashSet<string>
        {
            ".txt", ".csv", ".md", ".json", ".xml"
        };

        public bool IsTextFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(extension)) return false;

            return TextExtensions.Contains(extension);
        }

        public bool IsZipFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(extension)) return false;

            return extension.Equals(".zip");
        }
    }
}
