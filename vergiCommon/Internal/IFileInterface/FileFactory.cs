using System.Xml;
using FileTypeChecker;
using vergiCommon.Internal.File;
using vergiCommon.Public;

namespace vergiCommon.Internal.IFileInterface
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
                    var line = reader.ReadLine();
                    if (line == null) break;
                    lines.Add(line);
                }
            }

            // If text file
            var file = new TextFile(extension, lines);
            return file;
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/Comma-separated_values
        /// </summary>
        public ICsvFile ReadCsvFile(string filePath)
        {
            var file = Create(filePath, false);

            if (file.Extension != "csv")
                throw new ArgumentException($"Not valid csv file, file extension is: {file.Extension}");

            var lines = file.Lines.Where(l => !string.IsNullOrEmpty(l)).ToList();
            return CreateCsvFromTextFile(file);
        }

        private static readonly IEnumerable<string> CsvSeparators = new List<string> { ";", ",", ":", "  ", "   " };

        internal ICsvFile CreateCsvFromTextFile(IFile textFile)
        {
            var lines = textFile.Lines.Where(l => !string.IsNullOrEmpty(l)).ToList();
            var testLineAmount = Math.Min(lines.Count, 10);
            var resultSeparator = "";

            foreach (var separator in CsvSeparators)
            {
                var sepCountList = new List<int>();
                foreach (var i in Enumerable.Range(0, testLineAmount))
                {
                    var line = lines[i];
                    var split = line.Split(separator);
                    if (split.Length > 1)
                    {
                        var sepCount = split.Length - 1;
                        sepCountList.Add(sepCount);
                    }
                }

                if (!sepCountList.Any()) continue;
                var firstCount = sepCountList.First();
                if (sepCountList.Count == testLineAmount && sepCountList.All(c => c == firstCount))
                {
                    // Found suitable separator. Same amount of separators exist on each test line
                    resultSeparator = separator;
                    break;
                }
            }

            return ParseCsv(resultSeparator, textFile, lines);
        }

        private ICsvFile ParseCsv(string separator, IFile textFile, IEnumerable<string> lines)
        {
            // TODO how to deduce header?
            var data = new List<IReadOnlyList<string>>();
            foreach (var line in lines)
            {
                var row = line.Split(separator).ToList();
                data.Add(row);
            }

            var csv = new CsvFile(textFile)
            {
                Separator = separator,
                Headers = new List<string>(),
                Data = data
            };

            return csv;
        }
    }
}
