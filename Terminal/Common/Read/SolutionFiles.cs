namespace Terminal.Common.Read
{
    public class SolutionFiles
    {
        public static string GetSolutionPath()
        {
            var exePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            return Path.Combine(exePath, @"..\..\..\..");
        }

        /// <summary>
        /// Returns file path to ---
        /// </summary>
        /// <returns></returns>
        public static string GetFullPath(string fileName)
        {
            // Hack
            var projectPath = GetSolutionPath();
            return Path.GetFullPath(Path.Combine(projectPath, fileName));
        }

        public static string GetFullText(string fileName)
        {
            var filePath = GetFullPath(fileName);
            var text = File.ReadAllText(filePath);
            return text;
        }
    }

    public class ProjectFiles
    {
        public static string GetProjectPath()
        {
            var exePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            return Path.Combine(exePath, @"..\..\..");
        }
        
        /// <summary>
        /// Returns file path to ---
        /// </summary>
        /// <returns></returns>
        public static string GetFullPath(string fileName)
        {
            // Hack
            var projectPath = GetProjectPath();
            return Path.GetFullPath(Path.Combine(projectPath, fileName));
        }

        public static string GetFullText(string fileName)
        {
            var filePath = GetFullPath(fileName);
            var text = File.ReadAllText(filePath);
            return text;
        }
    }
}
