using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiCommon;

namespace vergiFinance.UnitTests
{
    [TestFixture]
    public class CommonTests
    {
        [Test]
        public void GetSolutionDir()
        {
            var result = GetPath.ThisSolution();
            var folderName = result.Split("\\").Last();

            Assert.That(folderName, Is.EqualTo("vergiFinance"));
        }

        [Test]
        public void GetProjectDir()
        {
            var result = GetPath.ThisProject();
            var folderName = result.Split("\\").Last();

            Assert.That(folderName, Is.EqualTo("vergiFinance.UnitTests"));
        }
    }
}
