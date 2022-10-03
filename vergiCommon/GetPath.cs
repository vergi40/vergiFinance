using System.Reflection;

namespace vergiCommon
{
    public static class GetPath
    {
        public static string MyDocuments()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public static string MyDocumentsSubFolder(string folderName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), folderName);
        }

        public static string Desktop()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }

        /// <returns>Bin folder path of the calling program</returns>
        public static string ThisAssembly()
        {
            return Environment.CurrentDirectory;
            //return Assembly.GetExecutingAssembly().
        }

        /// <returns>Project folder path of the calling program</returns>
        public static string ThisProject()
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            var projectName = callingAssembly.GetName().Name + ".csproj";

            var projectFolderPath = TravelParentsUntilFileFound(projectName, ThisAssembly());
            return projectFolderPath;
        }

        /// <returns>Solution folder path of the calling program</returns>
        public static string ThisSolution()
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            var projectName = callingAssembly.GetName().Name + ".csproj";

            var projectFolderPath = TravelParentsUntilFileFound(projectName, ThisAssembly());
            var solutionFolderPath = Directory.GetParent(projectFolderPath).FullName;

            return solutionFolderPath;
        }

        /// <returns>Return folder path where file was found</returns>
        private static string TravelParentsUntilFileFound(string fileName, string startFolder)
        {
            var current = Directory.GetParent(startFolder);
            for (int i = 0; i < 10; i++)
            {
                if (current == null) break;

                if (current.EnumerateFiles().Any(x => x.Name.Equals(fileName)))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            throw new ArgumentException($"Could not find file traveling the folder tree. File name: {fileName}");
        }
    }
}
