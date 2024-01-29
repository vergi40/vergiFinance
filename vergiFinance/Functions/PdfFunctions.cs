using System.Text.RegularExpressions;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace vergiFinance.Functions
{
    /// <summary>
    /// Non-destructive pdf reader/creator
    /// </summary>
    public class PdfFunctions
    {
        private List<string> _output { get; } = new List<string>();

        private void WriteLine(string text = "")
        {
            Console.WriteLine(text);
            _output.Add(text);
        }

        /// <summary>
        /// Combine all pdfs and images from folder to single pdf. Expects that files have date prefix
        /// such as MMDD_filename.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resultFileName">Result file name will be format MM_(filename).pdf</param>
        public void CombineFolderContentToSinglePdf(string path, string resultFileName)
        {
            CombineFolderContentToSinglePdf(path, resultFileName, out _);
        }

        /// <summary>
        /// Combine all pdfs and images from folder to single pdf. Expects that files have date prefix
        /// such as MMDD_filename.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resultFileName">Result file name will be format MM_(filename).pdf</param>
        /// <param name="output">Console output</param>
        public void CombineFolderContentToSinglePdf(string path, string resultFileName, out List<string> output)
        {
            output = new List<string>();
            WriteLine();
            WriteLine("----------------");
            WriteLine("PdfFunctions");
            WriteLine($@"Dir contents in : {path}");

            WriteLine();

            var fileNames = CollectFileNames(path);
            var prefix = Path.GetFileName(fileNames.First()).Substring(0, 2);
            WriteLine();
            var inputDocs = CollectInput(fileNames);
            var outputDoc = GenerateOutputDoc(inputDocs);

            var outputName = Path.Combine(path, $"{prefix}_{resultFileName}.pdf");
            outputDoc.Save(outputName);

            WriteLine($"Pdf combined and created to path: {outputName}");
            output = _output;
        }

        private List<string> CollectFileNames(string path)
        {
            var pattern = @"[0-9]{4}";
            var fileNames = new List<string>();
            foreach (var fileName in Directory.EnumerateFiles(path))
            {
                var isMatch = Regex.Match(fileName, pattern, RegexOptions.IgnoreCase);
                if (fileName.Length <= 5 || !isMatch.Success)
                {
                    WriteLine($"INCORRECT SYNTAX: File skipped: {Path.GetFileName(fileName)}");
                    continue;
                }

                fileNames.Add(fileName);
                WriteLine("   " + Path.GetFileName(fileName));
            }
            fileNames.Sort();
            return fileNames;
        }

        private List<PdfDocument> CollectInput(List<string> fileNames)
        {
            var result = new List<PdfDocument>();
            foreach (var fileName in fileNames)
            {
                try
                {
                    var extension = Path.GetExtension(fileName);
                    if (extension.Equals(".pdf"))
                    {
                        var importedPdf = PdfReader.Open(fileName, PdfDocumentOpenMode.Import);
                        importedPdf.Info.Title = Path.GetFileName(fileName);
                        result.Add(importedPdf);
                    }
                    else
                    {
                        var pdfFromImage = GeneratePdfFromImage(fileName);

                        var dataStream = new MemoryStream();
                        pdfFromImage.Save(dataStream);

                        result.Add(PdfReader.Open(dataStream, PdfDocumentOpenMode.Import));
                        dataStream.Close();
                    }
                }
                catch (Exception e)
                {
                    WriteLine($"Error generating pdf. File name: {Path.GetFileName(fileName)} {e.ToString()}");
                    WriteLine("WARNING: Skipping this file, continuing with rest of files. Note that this is missing in output pdf.");
                }
            }

            return result;
        }

        private PdfDocument GeneratePdfFromImage(string fileName)
        {
            var image = XImage.FromFile(fileName);
            var output = new PdfDocument();
            output.Info.Title = Path.GetFileName(fileName);
            var page = new PdfPage(output);
            var graphics = XGraphics.FromPdfPage(page);

            var pointMargin = 20;
            var pageW = page.Width - XUnit.FromPoint(pointMargin * 2);
            var pageH = page.Height - XUnit.FromPoint(pointMargin * 2);

            var imageW = XUnit.FromPoint(image.PointWidth);
            var imageH = XUnit.FromPoint(image.PointHeight);

            // Scale to max document size if image is larger
            var factorW = 1.0;
            var factorH = 1.0;
            if (imageW > pageW)
            {
                factorW = imageW / pageW;
            }
            if (imageH > pageH)
            {
                factorH = imageH / pageH;
            }

            var factor = Math.Max(factorW, factorH);
            if (factor <= 0) throw new InvalidOperationException("Tried to divide with zero");


            //double x = (250 - image.PixelWidth * 72 / image.HorizontalResolution) / 2;
            graphics.DrawImage(image, pointMargin, pointMargin, imageW.Point / factor, imageH.Point / factor);

            output.PageLayout = PdfPageLayout.OneColumn;
            output.AddPage(page);
            return output;
        }

        private PdfDocument GenerateOutputDoc(List<PdfDocument> inputDocs)
        {
            var output = new PdfDocument();
            output.PageLayout = PdfPageLayout.OneColumn;

            foreach (var inputDoc in inputDocs)
            {
                for (int i = 0; i < inputDoc.PageCount; i++)
                {
                    var page = output.AddPage(inputDoc.Pages[i]);
                    AddTitleAndPageNumber(page, inputDoc.Info.Title, i, inputDoc.PageCount);
                }
            }

            return output;
        }

        private void AddTitleAndPageNumber(PdfPage page, string fullPath, int pageIndex, int pageCount)
        {
            var graphics = XGraphics.FromPdfPage(page);
            var font = new XFont("consolas", 10);

            var box = page.MediaBox.ToXRect();
            box.Inflate(0, -10);

            var fileName = Path.GetFileName(fullPath);
            graphics.DrawString($"{fileName}  page ({pageIndex + 1}/{pageCount})", font, XBrushes.Red, box, XStringFormats.TopCenter);
        }
    }
}
