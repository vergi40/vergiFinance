using System.Reflection;
using vergiCommon.Internal;

namespace vergiCommon
{
    /// <summary>
    /// Interface path methods
    /// </summary>
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

            var projectFolderPath = PathUtils.TravelParentsUntilFileFound(projectName, ThisAssembly());
            return projectFolderPath;
        }

        /// <returns>Solution folder path of the calling program</returns>
        public static string ThisSolution()
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            var projectName = callingAssembly.GetName().Name + ".csproj";

            var projectFolderPath = PathUtils.TravelParentsUntilFileFound(projectName, ThisAssembly());
            var solutionFolderPath = PathUtils.TravelParentsUntilSlnFileFound(projectFolderPath);

            return solutionFolderPath;
        }
    }
}
