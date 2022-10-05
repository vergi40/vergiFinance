using vergiCommon;

namespace vergiFinance.Model
{
    public static class Constants
    {
        public static string MyDocumentsTempLocation =>
            Path.Combine(GetPath.MyDocumentsSubFolder("TempFinance"));
    }
}
