using vergiCommon;

namespace vergiFinance.UnitTests
{
    internal static class TestUtils
    {
        public static string GetResourcesPath()
        {
            var resFolder = Path.Combine(GetPath.ThisProject(), "Resources");
            return resFolder;
        }
    }
}
