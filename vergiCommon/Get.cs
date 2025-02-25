﻿using vergiCommon.Internal.File;
using vergiCommon.Internal.IFileInterface;
using vergiCommon.Internal.Input;
using vergiCommon.Public;

namespace vergiCommon
{
    /// <summary>
    /// Interface methods
    /// </summary>
    public static class Get
    {
        /// <summary>
        /// Read input stream. Either one key, or until ENTER is pressed.
        /// </summary>
        /// <param name="selectionModeOn">User selects one character instead of typing full string and ENTER.</param>
        /// <returns></returns>
        public static IInput ReadInput(bool selectionModeOn)
        {
            var readUtil = new Read();

            if (selectionModeOn)
            {
                var input = readUtil.ReadInputKey();
                readUtil.LineEnd();
                return input;
            }
            // User presses ENTER in the end of input
            else
            {
                return readUtil.ReadInputString();
            }
        }

        /// <summary>
        /// Read file (text/image/binary). Return object has basic file properties.
        /// Supports file validation.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="trustFileExtension">
        /// Analyzes file content before fully reading that it's recognizable
        /// </param>
        /// <returns></returns>
        public static IFile ReadFile(string filePath, bool trustFileExtension = true)
        {
            var fileFactory = new FileFactory();

            return fileFactory.CreateAsync(filePath, trustFileExtension).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Read file (text/image/binary). Return object has basic file properties.
        /// Supports file validation.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="trustFileExtension">
        /// Analyzes file content before fully reading that it's recognizable
        /// </param>
        /// <returns></returns>
        public static async Task<IFile> ReadFileAsync(string filePath, bool trustFileExtension = true)
        {
            var fileFactory = new FileFactory();

            return await fileFactory.CreateAsync(filePath, trustFileExtension);
        }

        /// <summary>
        /// Read comma-separated-values file. Provides simple data-instance where it should be
        /// easy to utilize parsed data.
        /// </summary>
        public static ICsvFile ReadCsvFile(string filePath)
        {
            var fileFactory = new FileFactory();

            return fileFactory.ReadCsvFile(filePath);
        }

        /// <summary>
        /// Read comma-separated-values file. Provides simple data-instance where it should be
        /// easy to utilize parsed data.
        /// </summary>
        public static async Task<ICsvFile> ReadCsvFileAsync(string filePath)
        {
            var fileFactory = new FileFactory();

            return await fileFactory.ReadCsvFileAsync(filePath);
        }

        public static IFile ReadSingleFileFromZip(string zipPath)
        {
            return ReadUtils.ReadFirstFromZipFile(zipPath);
        }

        public static IFile ReadFileFromZip(string zipPath, string fileName)
        {
            return ReadUtils.ReadFromZipFile(zipPath, fileName);
        }

        /// <summary>
        /// Return file names (including extensions)
        /// </summary>
        public static IReadOnlyList<string> ReadFileNamesFromZip(string zipPath)
        {
            return ReadUtils.ReadFileNamesFromZipFile(zipPath);
        }
    }
}
