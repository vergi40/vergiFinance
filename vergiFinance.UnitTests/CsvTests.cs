using Moq;
using Shouldly;
using vergiCommon.Internal.IFileInterface;
using vergiCommon.Internal.Input;
using vergiCommon.Public;

namespace vergiFinance.UnitTests
{
    [TestFixture]
    internal class CsvTests
    {
        private Mock<IFile> _textFile;
        private vergiCommon.Internal.IFileInterface.FileFactory _fileFactory;

        [SetUp]
        public void Setup()
        {
            _textFile = new Mock<IFile>();
            _textFile.Setup(f => f.Extension).Returns("csv");
            _textFile.Setup(f => f.FilePath).Returns("aaa.csv");
            _textFile.Setup(f => f.Content).Returns("");

            _fileFactory = new FileFactory();
        }

        [Test]
        public void Semicolon_ShouldMatch()
        {
            CreateAndAssert(new List<string>
            {
                "aaa;bbb;ccc",
                "ddd;eee;fff"
            });
        }

        [Test]
        public void Colon_ShouldMatch()
        {
            CreateAndAssert(new List<string>
            {
                "aaa:bbb:ccc",
                "ddd:eee:  fff"
            });
        }

        [Test]
        public void Comma_ShouldMatch()
        {
            CreateAndAssert(new List<string>
            {
                "aaa,bbb,ccc",
                "ddd,eee,fff   "
            });
        }

        [Test]
        public void WhiteSpaces_ShouldMatch()
        {
            CreateAndAssert(new List<string>
            {
                "aaa  bbb  ccc",
                "ddd  eee  fff "
            });
        }

        private void CreateAndAssert(List<string> lines)
        {
            SetupLines(lines);

            var result = _fileFactory.CreateCsvFromTextFile(_textFile.Object);
            AssertAbc(result);
        }

        private void SetupLines(List<string> lines)
        {
            _textFile.Setup(f => f.Lines).Returns(lines);
        }

        private void AssertAbc(ICsvFile file)
        {
            var row1 = file.Data[0];
            row1[0].ShouldBe("aaa");
            row1[1].ShouldBe("bbb");
            row1[2].ShouldBe("ccc");

            var row2 = file.Data[1];
            row2[0].ShouldBe("ddd");
            row2[1].ShouldBe("eee");
            row2[2].Trim().ShouldBe("fff");
        }
    }
}
