namespace vergiCommon
{
    public class Constants
    {
        public static string CryptographyTempFileLocation => throw new NotImplementedException();
        public static string CryptographyTempFileName => "encryptedData";

        public static string DatabaseFileLocation => throw new NotImplementedException();

        public static string MyDocumentsTempLocation =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TempFinance");


    }
}
